using htmx_trello.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmx_trello.Pages;

public class Board(BoardService boardService): PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }
    
    public BoardDto Current { get; private set; } = BoardDto.Empty;
    
    public void OnGet()
    {
        var board = boardService.GetById(Id);

        if (board is null)
        {
            // handle not found
            Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        Current = board;
    }
}