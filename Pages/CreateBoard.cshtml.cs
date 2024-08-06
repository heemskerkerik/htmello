using Htmx;
using htmx_trello.Data;
using htmx_trello.Pages.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmx_trello.Pages;

public class CreateBoardModel(BoardService boardService): PageModel, ICreateBoardModel
{
    public void OnPost()
    {
        var newBoard = boardService.Add(Board.Name, Board.Color);
        string newBoardUrl = Url.Page("Board", new { newBoard.Id })!;

        if (Request.IsHtmx())
        {
            Response.Htmx(h => h.Redirect(newBoardUrl));
        }
        else
        {
            Response.Redirect(newBoardUrl);
        }
    }

    [BindProperty] public CreateBoardRequest Board { get; set; } = new("", "");
}