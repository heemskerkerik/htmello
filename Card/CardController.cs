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

        Response.Htmx(h => h.WithTrigger($"cardAdded:{laneId:D}").WithTrigger("cardAdded"));

        return View("_Card", card);
    }

    [HttpDelete("/boards/{boardId:guid}/cards/{cardId:guid}")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteCard(Guid boardId, Guid cardId)
    {
        var card = boardService.GetCardById(boardId, cardId);

        if (card is null)
            return NotFound();
        
        boardService.DeleteCard(boardId, cardId);
        
        Response.Htmx(h => h.WithTrigger($"cardRemoved:{card.LaneId:D}").WithTrigger("cardRemoved"));

        // need to return 200 with an empty body to satisfy htmx's interpretation of HTTP/HTML spec
        // 204 might be more appropriate, but htmx literally interprets that as 'do nothing', including swapping
        return StatusCode(StatusCodes.Status200OK);
    }
}