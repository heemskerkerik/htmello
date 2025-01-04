namespace htmello.Data;

public interface IBoardService
{
    BoardDto? GetById(Guid boardId);
    BoardDto Add(string name, string color);
    IReadOnlyCollection<BoardDto> GetAll();
    void SetBoardName(Guid id, string name);
    LaneDto AddLane(Guid boardId, string laneName);
    CardDto AddCard(Guid boardId, Guid laneId, string cardName);
    IReadOnlyCollection<Guid> SortCards(Guid boardId, Guid laneId, IReadOnlyCollection<Guid> cards);
    void DeleteCard(Guid boardId, Guid cardId);
    CardDto? GetCardById(Guid boardId, Guid cardId);
    bool DoesLaneExistByName(Guid boardId, string laneName);
}