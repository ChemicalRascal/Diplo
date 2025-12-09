using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BSBackupSystem.Data;
using BSBackupSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace BSBackupSystem.Areas.Game.Pages;

public class IndexModel(AppDbContext appDb, DiploDataManager gameManager) : PageModel
{
    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    [ViewData]
    public required List<GameModel> CurrentGames { get; set; } = [];

    [BindProperty]
    public required IndexSubmission Input { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        CurrentGames = await appDb.Games.Select(g =>
            new GameModel()
            {
                Id = g.Id,
                Uri = g.Uri,
                CreationTime = g.CreationTime,
            }).ToListAsync();
        Input = new(string.Empty);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = await gameManager.RegisterGameAsync(Input.UrlToSubmit);

        if (!success)
        {
            StatusMessage = "Error: Game not tracked. (Is it already in the list?)";
        }
        else
        {
            StatusMessage = "Tracking your game.";
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var success = await gameManager.DeleteGameAsync(id);

        if (!success)
        {
            StatusMessage = "Error: Game not deleted.";
        }
        else
        {
            StatusMessage = "Game deleted.";
        }

        return await OnGetAsync();
    }

    public record IndexSubmission(string UrlToSubmit);

    public record GameModel
    {
        internal Guid Id = Guid.Empty;
        internal string Uri = string.Empty;
        internal DateTimeOffset CreationTime = DateTimeOffset.UnixEpoch;
    }
}