using System.Collections.Immutable;

namespace htmello.Data;

public record BoardDto(Guid BoardId, string Name, string Color, DateTimeOffset Created, ImmutableList<LaneDto> Lanes)
{
    public static BoardDto Empty { get; } = new(Guid.Empty, "", "", DateTimeOffset.MinValue, []);

    public int CardCount => Lanes.Select(l => l.Cards.Count).Sum();
}