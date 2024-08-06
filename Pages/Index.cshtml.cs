using htmx_trello.Data;
using htmx_trello.Pages.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmx_trello.Pages;

public class IndexModel(BoardService boardService): PageModel, ICreateBoardModel
{
    public void OnGet()
    {
        AllBoards = boardService.GetAll();
    }

    public CreateBoardRequest Board { get; } = new("", "#6968cd");
    public IReadOnlyCollection<BoardDto> AllBoards { get; private set; } = [];
}