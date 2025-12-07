using BSBackupSystem.Model.Diplo;
using BSBackupSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BSBackupSystem.Areas.Game.Pages;

public class GameModel(DiploDataManager dataManager) : PageModel
{
    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    [BindProperty]
    public required GameViewModel GameData { get; set; }

    [BindProperty]
    public OrdersViewModel?[,] OrdersGrid { get; set; } = new OrdersViewModel?[0, 0];

    [BindProperty]
    public (int Year, string Season)[] GridHeaders { get; set; } = [];

    public async Task<IActionResult> OnGet(Guid id)
    {
        var game = await dataManager.GetGameAsync(id);
        if (game is null)
        {
            StatusMessage = "Error: No such game.";
            return RedirectToPage("Index");
        }


        if (!game.MoveSets.Any())
        {
            StatusMessage = "Error: Game has no turns.";
            GameData = new(
                game.Id,
                game.CreationTime.LocalDateTime.ToString("g"),
                game.Uri);
            return Page();
        }

        // TODO: Do this literally any other way
        // Maybe something involving tree traversal and emitting
        // lines of movesets? Might need movesets to be linked
        // the other way, from first to last.

        var vertIndexes = new Dictionary<Guid,int>();
        var dependents = new Dictionary<Guid, List<Guid>>();

        var orderColumns = game.MoveSets
            .OrderBy(ms => ms.FirstSeen)
            .GroupBy(ms => (ms.Year, ms.SeasonIndex),
                     ms => ms)
            .OrderBy(c => c.Key.Year)
            .ThenBy(c => c.Key.SeasonIndex);

        /* Work out where everything goes in a grid.
         * Every dependent move must be on the same row or
         * lower than its parent move, but similarly each
         * parent move must move down to the first child's row.
         */

        var firstMoveSet = orderColumns.First().First();
        vertIndexes[firstMoveSet.Id] = 0;
        GameData = new(
            game.Id,
            game.CreationTime.LocalDateTime.ToString("g"),
            game.Uri,
            firstMoveSet.FirstSeen?.LocalDateTime.ToString("g"));

        foreach (var column in orderColumns)
        {
            Guid previousId = Guid.Empty;
            foreach (var orderSet in column)
            {
                if (!vertIndexes.ContainsKey(orderSet.Id))
                {
                    vertIndexes[orderSet.Id] =
                        Math.Max(
                            vertIndexes.GetValueOrDefault(previousId, -1) + 1,
                            (orderSet.PreviousSet?.Id is not null
                                ? vertIndexes[orderSet.PreviousSet.Id] : 0)
                        );
                }

                if (orderSet.PreviousSet is not null)
                {
                    if (!dependents.ContainsKey(orderSet.PreviousSet.Id))
                    {
                        dependents[orderSet.PreviousSet.Id] = new();
                    }
                    dependents[orderSet.PreviousSet.Id].Add(orderSet.Id);
                }

                previousId = orderSet.Id;
            }
        }

        foreach (var column in orderColumns.Reverse().Skip(1))
        {
            foreach (var orderSet in column)
            {
                if (!dependents.ContainsKey(orderSet.Id))
                {
                    continue;
                }

                vertIndexes[orderSet.Id] = dependents[orderSet.Id]
                    .Select(depId => vertIndexes[depId]).Min();
            }
        }

        OrdersGrid = new OrdersViewModel?[orderColumns.Count(),vertIndexes.Values.Max()+1];
        GridHeaders = new (int Year, string Season)[orderColumns.Count()];

        var columnIndex = 0;
        foreach (var column in orderColumns)
        {
            GridHeaders[columnIndex] = (column.First().Year, column.First().SeasonName);
            foreach (var orderSet in column)
            {
                OrdersGrid[columnIndex, vertIndexes[orderSet.Id]]
                    = new()
                    {
                        Id = orderSet.Id,
                        State = orderSet.State,
                        FirstSeen = orderSet.FirstSeen!
                            .Value.LocalDateTime
                            .TimeOfDay.ToString(@"hh\:mm")
                    };
            }
            columnIndex++;
        }

        return Page();
    }

    public class OrdersViewModel
    {
        public string State { get; set; } = string.Empty;
        public string FirstSeen { get; set; } = string.Empty;
        public Guid Id { get; set; } = Guid.Empty;
    }

    public record GameViewModel(Guid Id, string StartTime, string Url, string? FirstScrape = null);
}