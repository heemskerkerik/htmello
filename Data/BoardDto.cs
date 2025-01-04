using System.Collections.Immutable;

namespace htmx_trello.Data;

public record BoardDto(Guid Id, string Name, string Color, DateTimeOffset Created, ImmutableList<LaneDto> Lanes)
{
    public static BoardDto Empty { get; } = new(Guid.Empty, "", "", DateTimeOffset.MinValue, []);
}