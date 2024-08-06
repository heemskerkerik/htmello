using htmx_trello.Data;
using htmx_trello.Pages.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmx_trello.Pages;

public class EditBoardName(BoardService boardService): PageModel
{
    public void OnGet()
    {
        Current = boardService.GetById(Id) ?? throw new Exception($"Board {Id} not found");
    }

    public void OnPost()
    {
        boardService.SetBoardName(Id, Board.Name);
        
    }

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    [BindProperty] public EditBoardNameRequest Board { get; set; } = new("");
    public BoardDto Current { get; private set; } = BoardDto.Empty;
}