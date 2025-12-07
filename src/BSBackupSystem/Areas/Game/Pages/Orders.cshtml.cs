using BSBackupSystem.Model.Diplo;
using BSBackupSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BSBackupSystem.Areas.Game.Pages;

public class OrdersModel(DiploDataManager dataManager) : PageModel
{
    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    [BindProperty]
    public required TurnViewModel TurnData { get; set; }

    [BindProperty]
    public required Guid GameId { get; set; }

    public async Task<IActionResult> OnGet(Guid gameId, Guid id)
    {
        GameId = gameId;

        var orderSet = await dataManager.GetMovesAsync(gameId, id);
        if (orderSet is null)
        {
            StatusMessage = "Error: No such set of orders.";
            return RedirectToPage("Game", gameId);
        }

        if (!orderSet.Orders.Any())
        {
            StatusMessage = "Error: Set of orders has no orders.";
            TurnData = new(orderSet.Year, orderSet.SeasonName, []);
            return Page();
        }

        TurnData = new(orderSet.Year, orderSet.SeasonName,
            orderSet.Orders.GroupBy(o => o.Player)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(o => new UnitOrderViewModel(o.ToString())).ToList())
            );

        return Page();
    }

    public record TurnViewModel(int Year, string Season, Dictionary<string, List<UnitOrderViewModel>> Orders);

    public record UnitOrderViewModel(string Order);
}