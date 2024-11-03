namespace htmello.Data;

public record CardDto(Guid CardId, string Title, Guid BoardId, Guid LaneId);