using htmello.Data;
using Microsoft.AspNetCore.Mvc;

namespace htmello.Board;

public class BoardCardCountViewComponent(BoardService boardService): ViewComponent
{
    public IViewComponentResult Invoke(Guid boardId, bool outOfBandSwap = false)
    {
        var board = boardService.GetById(boardId)
                 ?? throw new Exception($"Could not find board {boardId}");

        ViewData["OutOfBandSwap"] = outOfBandSwap;

        return View(board);
    }
}