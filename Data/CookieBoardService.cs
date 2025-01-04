using System.Collections.Immutable;

namespace htmello.Data;

public class CookieBoardService(IHttpContextAccessor httpContextAccessor) : IBoardService
{
    public BoardDto? GetById(Guid boardId)
    {
        var definition = GetBoardDefinition(boardId);

        if (definition is null)
            return null;

        return new(boardId, definition.Value.name, definition.Value.color, DateTimeOffset.MinValue, GetLanes(boardId));
    }

    public BoardDto Add(string name, string color)
    {
        var boardIds = GetBoardIds();

        Guid boardId = Guid.NewGuid();

        string newBoardIds = string.Join(
            ",",
            boardIds.Append(boardId)
                    .Select(id => id.ToString("N"))
        );

        var boardData = EncodeBoardData(name, color);

        SetCookie("Boards", newBoardIds);
        SetCookie($"Boards-{boardId:N}", boardData);

        return new(boardId, name, color, DateTimeOffset.MinValue, ImmutableList<LaneDto>.Empty);
    }

    private void SetCookie(string name, string value)
    {
        httpContextAccessor.HttpContext!.Response.Cookies.Append(
            name,
            value,
            new()
            {
                Path = "/",
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
            }
        );
        _cookies[name] = value;
    }

    private static string EncodeBoardData(string name, string color) => Uri.EscapeDataString(name) + ";" + color;

    public IReadOnlyCollection<BoardDto> GetAll()
    {
        return GetBoardIds()
              .Select(id => GetById(id) ?? throw new($"Cannot get board {id:N}"))
              .ToList();
    }

    private IReadOnlyCollection<Guid> GetBoardIds()
    {
        if (!TryGetCookie("Boards", out var value) || string.IsNullOrWhiteSpace(value))
            return Array.Empty<Guid>();

        return value.Split(",").Select(Guid.Parse).ToList();
    }

    private bool TryGetCookie(string name, out string value)
    {
        value = "";

        if (!_cookies.TryGetValue(name, out var cookie))
        {
            if (httpContextAccessor.HttpContext!.Request.Cookies.TryGetValue(name, out cookie))
                _cookies[name] = cookie;
            else
                return false;
        }

        value = cookie;
        return true;
    }

    private (string name, string color)? GetBoardDefinition(Guid boardId)
    {
        if (!TryGetCookie($"Boards-{boardId:N}", out var value))
            return null;

        var parts = value.Split(';');

        return (Uri.UnescapeDataString(parts[0]), parts[1]);
    }

    private ImmutableList<LaneDto> GetLanes(Guid boardId)
    {
        var definitions = GetLaneDefinitions(boardId);

        return definitions.Select(def => GetLane(boardId, def.laneId, def.name)).ToImmutableList();
    }

    private IReadOnlyCollection<(Guid laneId, string name)> GetLaneDefinitions(Guid boardId)
    {
        if (!TryGetCookie($"Lanes-{boardId:N}", out var value) || string.IsNullOrWhiteSpace(value))
            return Array.Empty<(Guid laneId, string name)>();

        return value.Split(',')
                    .Select(def =>
                            {
                                var parts = def.Split('=');

                                return (laneId: Guid.Parse(parts[0]), name: Uri.UnescapeDataString(parts[1]));
                            })
                    .ToList();
    }

    private LaneDto GetLane(Guid boardId, Guid laneId, string name)
    {
        return new(laneId, name, GetCards(boardId, laneId), boardId);
    }

    private ImmutableList<CardDto> GetCards(Guid boardId, Guid laneId)
    {
        if (!TryGetCookie($"Cards-{laneId:N}", out var value) || string.IsNullOrWhiteSpace(value))
            return ImmutableList<CardDto>.Empty;

        var definitions =
            value.Split(',')
                 .Select(def =>
                         {
                             var parts = def.Split('=');

                             return (cardId: Guid.Parse(parts[0]), title: Uri.UnescapeDataString(parts[1]));
                         })
                 .ToList();

        return definitions.Select(def => new CardDto(def.cardId, def.title, boardId, laneId))
                          .ToImmutableList();
    }

    public void SetBoardName(Guid id, string name)
    {
        var (_, color) = GetBoardDefinition(id)
                      ?? throw new($"Cannot find board {id:N}");

        SetCookie($"Boards-{id:N}", EncodeBoardData(name, color));
    }

    public LaneDto AddLane(Guid boardId, string laneName)
    {
        var definitions = GetLaneDefinitions(boardId);

        Guid laneId = Guid.NewGuid();

        string laneCookie = string.Join(
            ",",
            definitions.Append((laneId, name: laneName))
                       .Select(def => EncodeLaneDefinition(def.laneId, def.name))
        );
        
        SetCookie($"Lanes-{boardId:N}", laneCookie);
        
        return new(laneId, laneName, ImmutableList<CardDto>.Empty, boardId);
    }

    private static string EncodeLaneDefinition(Guid laneId, string name) =>
        laneId.ToString("N") + "=" + Uri.EscapeDataString(name);

    public CardDto AddCard(Guid boardId, Guid laneId, string cardName)
    {
        var cards = GetCards(boardId, laneId);
        var newCard = new CardDto(Guid.NewGuid(), cardName, boardId, laneId);

        var newCards = cards.Append(newCard);
        StoreLaneCards(laneId, newCards);

        return newCard;
    }

    private void StoreLaneCards(Guid laneId, IEnumerable<CardDto> cards)
    {
        SetCookie(
            $"Cards-{laneId:N}",
            string.Join(
                ",",
                cards.Select(c => c.CardId.ToString("N") + "=" + Uri.EscapeDataString(c.Title))
            )
        );
    }

    public IReadOnlyCollection<Guid> SortCards(Guid boardId, Guid laneId, IReadOnlyCollection<Guid> cards)
    {
        var currentBoard = GetById(boardId) ?? throw new($"Cannot find board {boardId:N}");

        var currentLane = currentBoard.Lanes.SingleOrDefault(l => l.LaneId == laneId)
                       ?? throw new Exception($"Couldn't find lane {laneId} in board {boardId}.");

        // build a map of all cards in all lanes by ID, so we can easily use them to rebuild lanes
        var allCards = currentBoard.Lanes.SelectMany(l => l.Cards).ToDictionary(t => t.CardId);
        var allLanes = currentBoard.Lanes.ToDictionary(l => l.LaneId);

        var newCards = cards.Select(id => allCards[id]).ToList();

        // the current board is always assumed to be affected
        var affectedLaneIds = new HashSet<Guid> { laneId };

        for (int index = 0; index < newCards.Count; index++)
        {
            var card = newCards[index];

            // card came from this lane, no need to modify anything
            if (card.LaneId == laneId)
                continue;

            newCards[index] = card with { LaneId = laneId };

            var oldLane = allLanes[card.LaneId];
            oldLane = oldLane with { Cards = oldLane.Cards.Remove(card) };
            allLanes[card.LaneId] = oldLane;

            affectedLaneIds.Add(card.LaneId);
        }

        allLanes[laneId] = currentLane with { Cards = newCards.ToImmutableList() };
        
        foreach (Guid id in affectedLaneIds)
        {
            StoreLaneCards(id, allLanes[id].Cards);
        }
        
        return affectedLaneIds;
    }

    public void DeleteCard(Guid boardId, Guid cardId)
    {
        var card = GetCardById(boardId, cardId) ?? throw new($"Cannot find card {cardId:N} in board {boardId:N}");
        var laneCards = GetCards(boardId, card.LaneId);

        var newLaneCards = laneCards.Except([card]).ToList();
        
        StoreLaneCards(card.LaneId, newLaneCards);
    }

    public CardDto? GetCardById(Guid boardId, Guid cardId)
    {
        var board = GetById(boardId) ?? throw new($"Cannot find board {boardId:N}");
        
        return board.Lanes
                    .SelectMany(l => l.Cards)
                    .FirstOrDefault(c => c.CardId == cardId);
    }

    public bool DoesLaneExistByName(Guid boardId, string laneName) => 
        GetLaneDefinitions(boardId).Any(def => def.name == laneName);
    
    private readonly Dictionary<string, string> _cookies = new(StringComparer.OrdinalIgnoreCase);
}