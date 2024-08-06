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

        var lane = new LaneDto(Guid.NewGuid(), laneName, ImmutableList<CardDto>.Empty, boardId);

        var newBoard = currentBoard with { Lanes = currentBoard.Lanes.Add(lane) };
        
        if (!_boards.TryUpdate(boardId, newBoard, currentBoard))
            throw new InvalidOperationException("Failed to update board. Concurrency violation?");

        return lane;
    }

    public CardDto AddCard(Guid boardId, Guid laneId, string cardName)
    {
        if (!_boards.TryGetValue(boardId, out var currentBoard))
            throw new Exception($"Couldn't find board {boardId}.");

        var currentLane = currentBoard.Lanes.SingleOrDefault(l => l.LaneId == laneId)
                ?? throw new Exception($"Couldn't find lane {laneId} in board {boardId}.");

        var card = new CardDto(Guid.NewGuid(), cardName, boardId, laneId);
        var newLane = currentLane with { Cards = currentLane.Cards.Add(card) };
        var newBoard = currentBoard with { Lanes = currentBoard.Lanes.Replace(currentLane, newLane) };

        if (!_boards.TryUpdate(boardId, newBoard, currentBoard))
            throw new InvalidOperationException("Failed to update board. Concurrency violation?");

        return card;
    }

    public BoardDto SortCards(Guid boardId, Guid laneId, IReadOnlyCollection<Guid> cards)
    {
        if (!_boards.TryGetValue(boardId, out var currentBoard))
            throw new Exception($"Couldn't find board {boardId}.");

        var currentLane = currentBoard.Lanes.SingleOrDefault(l => l.LaneId == laneId)
                       ?? throw new Exception($"Couldn't find lane {laneId} in board {boardId}.");
        
        // build a map of all cards in all lanes by ID, so we can easily use them to rebuild lanes
        var allCards = currentBoard.Lanes.SelectMany(l => l.Cards).ToDictionary(t => t.CardId);
        var allLanes = currentBoard.Lanes.ToDictionary(l => l.LaneId);

        var newCards = cards.Select(id => allCards[id]).ToList();

        for (int index = 0; index < newCards.Count; index++)
        {
            var card = newCards[index];
            
            // card came from this lane, no need to modify anything
            if (card.LaneId == laneId)
                continue;
            
            newCards[index] = card with { LaneId = laneId };

            var oldLane = allLanes[card.LaneId];
            oldLane = oldLane with { Cards = oldLane.Cards.Remove(card) };
            allLanes[card.LaneId] = oldLane;
        }

        allLanes[laneId] = currentLane with { Cards = newCards.ToImmutableList() };
        var newLanes = currentBoard.Lanes.Select(l => allLanes[l.LaneId]).ToImmutableList();

        var newBoard = currentBoard with { Lanes = newLanes };
        
        if (!_boards.TryUpdate(boardId, newBoard, currentBoard))
            throw new InvalidOperationException("Failed to update board. Concurrency violation?");

        return newBoard;
    }

    private readonly ConcurrentDictionary<Guid, BoardDto> _boards = new();
}