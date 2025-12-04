using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BSBackupSystem.Data;
using BSBackupSystem.Services;

namespace BSBackupSystem.Areas.Game.Pages;

public class IndexModel(AppDbContext appDb, DiploDataManager gameManager) : PageModel
{
    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    [ViewData]
    public required List<GameModel> CurrentGames { get; set; } = [];

    [BindProperty]
    public required IndexSubmission Input { get; set; }

    public void OnGet()
    {
        CurrentGames = appDb.Games.Select(g =>
            new GameModel()
            {
                Uri = g.Uri,
                CreationTime = g.CreationTime,
            }).ToList();
        Input = new(string.Empty);
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

    public record IndexSubmission(string UrlToSubmit);

    public record GameModel
    {
        internal string Uri = string.Empty;
        internal DateTimeOffset CreationTime = DateTimeOffset.UnixEpoch;
    }
}