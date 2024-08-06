namespace htmx_trello.Data;

public record TicketDto(Guid TicketId, string Title, Guid BoardId, Guid LaneId);