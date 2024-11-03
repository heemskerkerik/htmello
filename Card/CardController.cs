using Htmx;
using htmello.Data;
using Microsoft.AspNetCore.Mvc;

namespace htmello.Card;

[Controller]
public class CardController(BoardService boardService): Controller
{
    [HttpPost("/boards/{boardId:guid}/lanes/{laneId:guid}")]
    public IActionResult AddCard(
        Guid boardId,
        Guid laneId,
        [FromForm] AddCardRequest request
    )
    {
        var card = boardService.AddCard(boardId, laneId, request.CardName);

        Response.Htmx(h => h.WithTrigger($"cardAdded:{laneId:D}").WithTrigger("cardAdded"));

        ViewData["RenderBoardCardCount"] = true;

        return View("_Card", card);
    }

    [HttpDelete("/boards/{boardId:guid}/cards/{cardId:guid}")]
    public IActionResult DeleteCard(Guid boardId, Guid cardId)
    {
        var card = boardService.GetCardById(boardId, cardId);

        if (card is null)
            return NotFound();

        boardService.DeleteCard(boardId, cardId);

        Response.Htmx(h => h.WithTrigger($"cardRemoved:{card.LaneId:D}").WithTrigger("cardRemoved"));

        return ViewComponent(
            "BoardCardCount",
            new { boardId, outOfBandSwap = true }
        );
    }
}