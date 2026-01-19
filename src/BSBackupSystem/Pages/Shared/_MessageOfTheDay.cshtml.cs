using BSBackupSystem.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BSBackupSystem.Pages.Shared;

public class MessageOfTheDayModel(AppDbContext appDb) : PageModel
{
    public void OnGet()
    {
    }
}
