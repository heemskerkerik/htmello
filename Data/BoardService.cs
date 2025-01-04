using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace htmx_trello.Data;

public class BoardService
{
    public BoardDto? GetById(Guid id) => _boards.GetValueOrDefault(id);

    public BoardDto Add(string name, string color)
    {
        var newBoard = new BoardDto(Guid.NewGuid(), name, color, DateTimeOffset.Now, ImmutableList<LaneDto>.Empty);

        if (!_boards.TryAdd(newBoard.BoardId, newBoard))
        {
            throw new InvalidOperationException("Failed to add board. GUID collision?");
        }

        return newBoard;
    }

    public IReadOnlyCollection<BoardDto> GetAll() => _boards.Values.OrderBy(b => b.Created).ToList();

    public void SetBoardName(Guid id, string name)
    {
        if (!_boards.TryGetValue(id, out var currentBoard))
            throw new Exception($"Couldn't find board {id}.");

        var newBoard = currentBoard with { Name = name };

        if (!_boards.TryUpdate(id, newBoard, currentBoard))
            throw new InvalidOperationException("Failed to update board. Concurrency violation?");
    }

    public LaneDto AddLane(Guid boardId, string laneName)
    {
        if (!_boards.TryGetValue(boardId, out var currentBoard))
            throw new Exception($"Couldn't find board {boardId}.");
        
        if (currentBoard.Lanes.Any(l => l.Name == laneName))
            throw new Exception($"Board {boardId} already has a lane called '{laneName}'.");

        var lane = new LaneDto(Guid.NewGuid(), laneName, ImmutableList<TicketDto>.Empty, boardId);

        var newBoard = currentBoard with { Lanes = currentBoard.Lanes.Add(lane) };
        
        if (!_boards.TryUpdate(boardId, newBoard, currentBoard))
            throw new InvalidOperationException("Failed to update board. Concurrency violation?");

        return lane;
    }

    public TicketDto AddTicket(Guid boardId, Guid laneId, string ticketName)
    {
        if (!_boards.TryGetValue(boardId, out var currentBoard))
            throw new Exception($"Couldn't find board {boardId}.");

        var currentLane = currentBoard.Lanes.SingleOrDefault(l => l.LaneId == laneId)
                ?? throw new Exception($"Couldn't find lane {laneId} in board {boardId}.");

        var ticket = new TicketDto(Guid.NewGuid(), ticketName, boardId, laneId);
        var newLane = currentLane with { Tickets = currentLane.Tickets.Add(ticket) };
        var newBoard = currentBoard with { Lanes = currentBoard.Lanes.Replace(currentLane, newLane) };

        if (!_boards.TryUpdate(boardId, newBoard, currentBoard))
            throw new InvalidOperationException("Failed to update board. Concurrency violation?");

        return ticket;
    }

    public BoardDto SortTickets(Guid boardId, IReadOnlyDictionary<Guid, IReadOnlyCollection<Guid>> ticketsInLanes)
    {
        if (!_boards.TryGetValue(boardId, out var currentBoard))
            throw new Exception($"Couldn't find board {boardId}.");
        
        // build a map of all tickets by ID, so we can easily use them to rebuild lanes
        var allTickets = currentBoard.Lanes.SelectMany(l => l.Tickets).ToDictionary(t => t.TicketId);

        var lanes = currentBoard.Lanes;

        // sort tickets for lanes that are in ticketsInLanes
        foreach (var pair in ticketsInLanes)
        {
            var currentLane = lanes.SingleOrDefault(l => l.LaneId == pair.Key)
                           ?? throw new Exception($"Couldn't find lane {pair.Key} in board {boardId}.");

            var lane = currentLane with { Tickets = pair.Value.Select(id => allTickets[id]).ToImmutableList() };
            lanes = lanes.Replace(currentLane, lane);
        }
        
        // other lanes must be empty
        foreach (var lane in lanes.Where(l => !ticketsInLanes.ContainsKey(l.LaneId) && !l.Tickets.IsEmpty))
        {
            var newLane = lane with { Tickets = ImmutableList<TicketDto>.Empty };
            lanes = lanes.Replace(lane, newLane);
        }

        var newBoard = currentBoard with { Lanes = lanes };
        
        if (!_boards.TryUpdate(boardId, newBoard, currentBoard))
            throw new InvalidOperationException("Failed to update board. Concurrency violation?");

        return newBoard;
    }

    public BoardDto SortTickets(Guid boardId, Guid laneId, IReadOnlyCollection<Guid> tickets)
    {
        if (!_boards.TryGetValue(boardId, out var currentBoard))
            throw new Exception($"Couldn't find board {boardId}.");

        var currentLane = currentBoard.Lanes.SingleOrDefault(l => l.LaneId == laneId)
                       ?? throw new Exception($"Couldn't find lane {laneId} in board {boardId}.");
        
        // build a map of all tickets in all lanes by ID, so we can easily use them to rebuild lanes
        var allTickets = currentBoard.Lanes.SelectMany(l => l.Tickets).ToDictionary(t => t.TicketId);
        var allLanes = currentBoard.Lanes.ToDictionary(l => l.LaneId);

        var newTickets = tickets.Select(id => allTickets[id]).ToList();

        for (int index = 0; index < newTickets.Count; index++)
        {
            var ticket = newTickets[index];
            
            // ticket came from this lane, no need to modify anything
            if (ticket.LaneId == laneId)
                continue;
            
            newTickets[index] = ticket with { LaneId = laneId };

            var oldLane = allLanes[ticket.LaneId];
            oldLane = oldLane with { Tickets = oldLane.Tickets.Remove(ticket) };
            allLanes[ticket.LaneId] = oldLane;
        }

        allLanes[laneId] = currentLane with { Tickets = newTickets.ToImmutableList() };
        var newLanes = currentBoard.Lanes.Select(l => allLanes[l.LaneId]).ToImmutableList();

        var newBoard = currentBoard with { Lanes = newLanes };
        
        if (!_boards.TryUpdate(boardId, newBoard, currentBoard))
            throw new InvalidOperationException("Failed to update board. Concurrency violation?");

        return newBoard;
    }

    private readonly ConcurrentDictionary<Guid, BoardDto> _boards = new();
}