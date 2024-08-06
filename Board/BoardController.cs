using Htmx;
using htmx_trello.Data;
using Microsoft.AspNetCore.Mvc;

namespace htmx_trello.Board;

[Controller]
public class BoardController(BoardService boardService): Controller
{
    [HttpGet("/boards/{boardId:guid}")]
    public IActionResult ViewBoard(Guid boardId)
    {
        var board = boardService.GetById(boardId);

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
        var newBoard = boardService.Add(request.BoardName, request.Color);
        string newBoardUrl = Url.Action("ViewBoard", "Board", new { boardId = newBoard.BoardId })!;

        Response.Htmx(h => h.Redirect(newBoardUrl));
        return Redirect(newBoardUrl);
    }

    [HttpGet("/boards/{boardId:guid}/editName")]
    public IActionResult ShowEditBoardName(Guid boardId)
    {
        var board = boardService.GetById(boardId);

        return View("_EditBoardName", board);
    }

    [HttpGet("/boards/{boardId:guid}/name")]
    public IActionResult ShowBoardName(Guid boardId)
    {
        var board = boardService.GetById(boardId);

        return View("_BoardName", board);
    }

    [HttpPut("/boards/{boardId:guid}/editName")]
    [ValidateAntiForgeryToken]
    public IActionResult EditBoardName(
        Guid boardId,
        [FromForm] EditBoardNameRequest request
    )
    {
        boardService.SetBoardName(boardId, request.BoardName);
        var editedBoard = boardService.GetById(boardId);

        return View("_BoardName", editedBoard);
    }

    [HttpGet("/boards/{boardId:guid}/addLaneForm")]
    public IActionResult ShowAddLaneForm(Guid boardId)
    {
        var board = boardService.GetById(boardId);

        return View("_AddLane", board);
    }
}