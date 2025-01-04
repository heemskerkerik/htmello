using Htmx;
using htmx_trello.Data;
using Microsoft.AspNetCore.Mvc;

namespace htmx_trello.Card;

[Controller]
public class CardController(BoardService boardService): Controller
{
    [HttpPost("/boards/{boardId:guid}/lanes/{laneId:guid}")]
    [ValidateAntiForgeryToken]
    public IActionResult AddCard(
        Guid boardId,
        Guid laneId,
        [FromForm] AddCardRequest request
    )
    {
        var card = boardService.AddCard(boardId, laneId, request.CardName);

        Response.Htmx(h => h.WithTrigger($"cardAdded:{laneId:D}"));

        return View("_Card", card);
    }
}