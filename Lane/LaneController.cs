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
    public IActionResult AddLane(
        Guid boardId,
        [FromForm] AddLaneRequest request
    )
    {
        if (boardService.DoesLaneExistByName(boardId, request.LaneName))
        {
            ModelState.AddModelError(nameof(AddLaneRequest.LaneName), "A lane with the same name already exists.");
            var model = new AddLaneModel(boardId, request.LaneName);

            // retarget response, so happy case is simpler
            Response.Htmx(
                h => h.Retarget("this")
                      .Reswap("outerHTML")
            );

            return View("_AddLaneForm", model);
        }

        var lane = boardService.AddLane(boardId, request.LaneName);

        // configure target/swap here, so we can keep the validation error case simpler
        Response.Htmx(
            h => h.WithTrigger("laneAdded")
        );

        return View("_Lane", lane);
    }

    [HttpPut("/boards/{boardId:guid}/lanes/{laneId:guid}/sortItems")]
    public IActionResult SortCards(
        Guid boardId,
        Guid laneId,
        [FromForm] IReadOnlyCollection<Guid> cards
    )
    {
        var affectedLaneIds = boardService.SortCards(boardId, laneId, cards);

        Response.Htmx(h =>
        {
            h.WithTrigger("cardsSorted");
            foreach (var id in affectedLaneIds)
                h.WithTrigger($"cardsSorted:{id}");
        });

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