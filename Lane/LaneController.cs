using System.Collections.Immutable;
using System.Net.Mime;
using Htmx;
using htmx_trello.Data;
using Microsoft.AspNetCore.Mvc;

namespace htmx_trello.Lane;

[Controller]
public class LaneController(BoardService boardService): Controller
{
    [HttpPost("/boards/{boardId:guid}/lanes")]
    [ValidateAntiForgeryToken]
    public IActionResult AddLane(
        Guid boardId,
        [FromForm] AddLaneRequest request
    )
    {
        var lane = boardService.AddLane(boardId, request.LaneName);
        
        Response.Htmx(h => h.WithTrigger("laneAdded"));
        return View("_Lane", lane);
    }

    [HttpGet("/boards/{boardId:guid}/lanes/{laneId:guid}/addCardForm")]
    public IActionResult ShowAddCardForm(
        Guid boardId,
        Guid laneId
    ) => View("_AddCard", new LaneDto(laneId, "", ImmutableList<CardDto>.Empty, boardId));

    [HttpPut("/boards/{boardId:guid}/lanes/{laneId:guid}/sortItems")]
    [ValidateAntiForgeryToken]
    public IActionResult SortCards(
        Guid boardId,
        Guid laneId,
        [FromForm] IReadOnlyCollection<Guid> cards
    )
    {
        boardService.SortCards(boardId, laneId, cards);
        
        Response.Htmx(h => h.WithTrigger("cardsSorted"));

        return NoContent();
    }
    
    [HttpGet("/boards/{boardId:guid}/lanes/{laneId:guid}/cardCount")]
    public IActionResult GetCardCount(Guid boardId, Guid laneId)
    {
        var board = boardService.GetById(boardId);

        if (board is null)
            return NotFound();

        var lane = board.Lanes.SingleOrDefault(l => l.LaneId == laneId);

        if (lane is null)
            return NotFound();

        return Content(lane.Cards.Count.ToString(), MediaTypeNames.Text.Html);
    }
}