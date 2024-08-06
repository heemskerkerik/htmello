using Htmx;
using htmx_trello.Data;
using Microsoft.AspNetCore.Mvc;

namespace htmx_trello.Board;

[Controller]
public class BoardController(BoardService boardService): Controller
{
    [HttpGet("/boards/{id:guid}")]
    public IActionResult ViewBoard(Guid id)
    {
        var board = boardService.GetById(id);

        if (board is null)
        {
            // handle not found
            return NotFound();
        }

        return View(board);
    }

    [HttpPost("/boards")]
    [ValidateAntiForgeryToken]
    public IActionResult CreateBoard([FromForm(Name = "Board")] CreateBoardRequest request)
    {
        var newBoard = boardService.Add(request.Name, request.Color);
        string newBoardUrl = Url.Action("ViewBoard", "Board", new { id = newBoard.Id })!;

        Response.Htmx(h => h.Redirect(newBoardUrl));
        return Redirect(newBoardUrl);
    }

    [HttpGet("/boards/{id:guid}/editName")]
    public IActionResult ShowEditBoardName(Guid id)
    {
        var board = boardService.GetById(id);

        return View("_EditBoardName", board);
    }

    [HttpPut("/boards/{id:guid}/editName")]
    [ValidateAntiForgeryToken]
    public IActionResult EditBoardName(
        Guid id,
        [FromForm] EditBoardNameRequest request
    )
    {
        boardService.SetBoardName(id, request.Name);
        var editedBoard = boardService.GetById(id);

        return View("_BoardName", editedBoard);
    }
}