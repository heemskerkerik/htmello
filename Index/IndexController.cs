using htmello.Data;
using Microsoft.AspNetCore.Mvc;

namespace htmello.Index;

[Controller]
public class IndexController(BoardService boardService): Controller
{
    [HttpGet("/")]
    public IActionResult Index()
    {
        var allBoards = boardService.GetAll();
        
        return View(new IndexModel(allBoards));
    }
}