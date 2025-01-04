using Htmx;
using htmx_trello.Data;
using Microsoft.AspNetCore.Mvc;

namespace htmx_trello.Ticket;

[Controller]
public class TicketController(BoardService boardService): Controller
{
    [HttpPost("/boards/{boardId:guid}/lanes/{laneId:guid}")]
    [ValidateAntiForgeryToken]
    public IActionResult AddTicket(
        Guid boardId,
        Guid laneId,
        [FromForm] AddTicketRequest request
    )
    {
        var ticket = boardService.AddTicket(boardId, laneId, request.Name);
        
        Response.Htmx(h => h.WithTrigger($"ticketAdded:{laneId:D}"));

        return View("_Ticket", ticket);
    }
}