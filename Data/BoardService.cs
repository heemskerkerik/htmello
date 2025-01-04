using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace htmx_trello.Data;

public class BoardService
{
    public BoardDto? GetById(Guid id) => _boards.GetValueOrDefault(id);

    public BoardDto Add(string name, string color)
    {
        var newBoard = new BoardDto(Guid.NewGuid(), name, color, DateTimeOffset.Now, ImmutableList<LaneDto>.Empty);

        if (!_boards.TryAdd(newBoard.Id, newBoard))
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

        var lane = new LaneDto(Guid.NewGuid(), laneName, boardId);

        var newBoard = currentBoard with { Lanes = currentBoard.Lanes.Add(lane) };
        
        if (!_boards.TryUpdate(boardId, newBoard, currentBoard))
            throw new InvalidOperationException("Failed to update board. Concurrency violation?");

        return lane;
    }

    private readonly ConcurrentDictionary<Guid, BoardDto> _boards = new();
}