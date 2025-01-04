using System.Collections.Immutable;

namespace htmello.Data;

public record LaneDto(Guid LaneId, string Name, ImmutableList<CardDto> Cards, Guid BoardId);