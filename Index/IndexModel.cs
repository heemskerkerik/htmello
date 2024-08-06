using htmx_trello.Board;
using htmx_trello.Data;

namespace htmx_trello.Index;

public record IndexModel(IReadOnlyCollection<BoardDto> AllBoards): ICreateBoardModel
{
    public CreateBoardRequest Board { get; } = new("", "#6968cd");
}