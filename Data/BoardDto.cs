namespace htmx_trello.Data;

public record BoardDto(Guid Id, string Name, string Color, DateTimeOffset Created, IReadOnlyList<LaneDto> Lanes)
{
    public static BoardDto Empty { get; } = new(Guid.Empty, "", "", DateTimeOffset.MinValue, []);
}