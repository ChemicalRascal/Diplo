using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BSBackupSystem.Model.App;

namespace BSBackupSystem.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterConfirmationModel(UserManager<User> userManager) : PageModel
{
    public async Task<IActionResult> OnGetAsync(string? email)
    {
        if (email is null)
        {
            return RedirectToPage("/Index");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return NotFound($"Unable to load user with email '{email}'.");
        }

        return Page();
    }
}
