using BSBackupSystem.Data;
using BSBackupSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BSBackupSystem.Areas.Game.Pages;

public class GameModel(AppDbContext appDb) : PageModel
{
    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public async Task<IActionResult> OnGet(Guid id)
    {
        var game = await appDb.Games.FirstOrDefaultAsync(g =>  g.Id == id);
        if (game is null)
        {
            StatusMessage = "Error: No Such Game.";
            return RedirectToPage("Index");
        }

        throw new NotImplementedException();
    }
}