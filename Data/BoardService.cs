using System.Collections.Concurrent;

namespace htmx_trello.Data;

public class BoardService
{
    public BoardDto? GetById(Guid id) => _boards.GetValueOrDefault(id);

    public BoardDto Add(string name, string color)
    {
        var newBoard = new BoardDto(Guid.NewGuid(), name, color, DateTimeOffset.Now, []);

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
            throw new Exception($"Could find board {id}.");

        var newBoard = currentBoard with { Name = name };

        if (!_boards.TryUpdate(id, newBoard, currentBoard))
            throw new InvalidOperationException("Failed to update board. Concurrency violation?");
    }

    private readonly ConcurrentDictionary<Guid, BoardDto> _boards = new();
}