using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BSBackupSystem.Data;
using BSBackupSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BSBackupSystem.Model.App;

namespace BSBackupSystem.Areas.Game.Pages;

public class IndexModel(AppDbContext appDb, DiploDataManager gameManager, UserManager<User> userManager) : PageModel
{
    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    [ViewData]
    public required List<GameModel> CurrentGames { get; set; } = [];

    [BindProperty]
    public required IndexSubmission Input { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var dbUser = await userManager.GetUserAsync(User);

        CurrentGames = await appDb.Games.Select(g =>
            new GameModel()
            {
                Id = g.Id,
                Uri = g.Uri,
                CreationTime = g.CreationTime,
                UserOwnsThisGame = g.Owner == dbUser,
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

        var dbUser = await userManager.GetUserAsync(User);
        if (dbUser is null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        var (success, reason) = await gameManager.RegisterGameAsync(Input.UrlToSubmit, dbUser);

        if (!success)
        {
            StatusMessage = $"Error: Game not tracked: {reason!}";
        }
        else
        {
            StatusMessage = "Tracking your game.";
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var dbUser = await userManager.GetUserAsync(User);
        if (dbUser is null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        var (success, reason) = await gameManager.DeleteGameAsync(id, dbUser);

        if (!success)
        {
            StatusMessage = $"Error: Game not deleted: {reason!}";
        }
        else
        {
            StatusMessage = "Game deleted.";
        }

        return Page();
    }

    public record IndexSubmission(string UrlToSubmit);

    public record GameModel
    {
        internal Guid Id = Guid.Empty;
        internal string Uri = string.Empty;
        internal DateTimeOffset CreationTime = DateTimeOffset.UnixEpoch;
        internal bool UserOwnsThisGame = false;
    }
}