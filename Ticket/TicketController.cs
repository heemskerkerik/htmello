using Microsoft.AspNetCore.Mvc;

namespace htmx_trello.Ticket;

[Controller]
public class TicketController: Controller
{
    [HttpPost("/boards/{boardId:guid}/lanes/{laneId:guid}")]
    [ValidateAntiForgeryToken]
    public IActionResult AddTicket(
        Guid boardId,
        Guid laneId,
        [FromForm] AddTicketRequest request
    )
    {
        throw new NotImplementedException();
    }
}