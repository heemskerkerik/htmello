using System.Net.Mime;
using htmx_trello.Data;
using htmx_trello.Lane;
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
            return NotFound();

        return View(board);
    }

    [HttpPost("/boards")]
    public IActionResult CreateBoard([FromForm(Name = "Board")] CreateBoardRequest request)
    {
        var newBoard = boardService.Add(request.BoardName, request.Color);
        string newBoardUrl = $"/boards/{newBoard.BoardId}"!;

        return Redirect(newBoardUrl);
    }

    [HttpGet("/boards/{boardId:guid}/editName")]
    public IActionResult ShowEditBoardName(Guid boardId)
    {
        var board = boardService.GetById(boardId);

        if (board is null)
            return NotFound();

        return View("_EditBoardName", board);
    }

    [HttpGet("/boards/{boardId:guid}/name")]
    public IActionResult ShowBoardName(Guid boardId)
    {
        var board = boardService.GetById(boardId);

        if (board is null)
            return NotFound();

        return View("_BoardName", board);
    }

    [HttpPut("/boards/{boardId:guid}/editName")]
    public IActionResult EditBoardName(
        Guid boardId,
        [FromForm] EditBoardNameRequest request
    )
    {
        boardService.SetBoardName(boardId, request.BoardName);
        var editedBoard = boardService.GetById(boardId);

        ViewData["RenderTitleTag"] = true;

        return View("_BoardName", editedBoard);
    }

    [HttpGet("/boards/{boardId:guid}/addLaneForm")]
    public IActionResult ShowAddLaneForm(Guid boardId)
    {
        var model = new AddLaneModel(boardId, "");
        return View("_AddLane", model);
    }

    [HttpGet("/boards/{boardId:guid}/cardCount")]
    public IActionResult GetCardCount(Guid boardId)
    {
        var board = boardService.GetById(boardId);

        if (board is null)
            return NotFound();

        return Content(board.CardCount.ToString(), MediaTypeNames.Text.Html);
    }
}