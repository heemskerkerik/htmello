using htmx_trello.Data;
using Microsoft.AspNetCore.Mvc;

namespace htmx_trello.Board;

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