using System.Collections.Immutable;

namespace htmx_trello.Data;

public record LaneDto(Guid LaneId, string Name, ImmutableList<TicketDto> Tickets, Guid BoardId);