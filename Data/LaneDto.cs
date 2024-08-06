using System.Collections.Immutable;

namespace htmx_trello.Data;

public record LaneDto(Guid LaneId, string Name, ImmutableList<CardDto> Cards, Guid BoardId);