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
        var lane = boardService.AddLane(boardId, request.Name);
        
        Response.Htmx(h => h.WithTrigger("laneAdded"));
        return View("_Lane", lane);
    }
}