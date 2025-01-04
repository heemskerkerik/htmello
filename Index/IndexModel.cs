using htmello.Board;
using htmello.Data;

namespace htmello.Index;

public record IndexModel(IReadOnlyCollection<BoardDto> AllBoards): ICreateBoardModel
{
    public CreateBoardRequest Board { get; } = new("", "#6968cd");
}