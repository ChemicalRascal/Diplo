using System.Text.RegularExpressions;
using BSBackupSystem.Data;
using BSBackupSystem.Model.App;
using BSBackupSystem.Model.Diplo;
using Microsoft.EntityFrameworkCore;

namespace BSBackupSystem.Services;

public partial class DiploDataManager(AppDbContext appDb)
{
    [GeneratedRegex(@"^http.*\.com/(?:sandbox|game)/([^/]*/)?\d{10,}/?")]
    private static partial Regex UrlSanitizationPattern();

    [GeneratedRegex(@"/(\d{10,})/")]
    private static partial Regex ForeignIdPattern();

    public async Task<(bool success, string? reason)> RegisterGameAsync(string url, User user)
    {
        var (properUrl, foreignId) = ExtractKeyValues(url);

        if (await appDb.Games.Where(g => g.ForeignId == foreignId).AnyAsync())
        {
            return (false, "Game already tracked.");
        }

        await appDb.Games.AddAsync(new()
        {
            Uri = properUrl,
            ForeignId = foreignId,
            CreationTime = DateTime.UtcNow,
            Owner = user,
        });
        await appDb.SaveChangesAsync();
        return (true, null);
    }

    private readonly IQueryable<Game> gamesQueriable = appDb.Games
        .Include(g => g.MoveSets)
        .ThenInclude(ms => ms.Orders);

    public async Task<Game?> GetGameAsync(string url)
    {
        var (_, foreignId) = ExtractKeyValues(url);

        return await gamesQueriable
            .Where(g => g.ForeignId == foreignId).FirstOrDefaultAsync();
    }

    public async Task<Game?> GetGameAsync(Guid id)
    {
        return await gamesQueriable
            .Where(g => g.Id == id).FirstOrDefaultAsync();
    }

    public async Task<MoveSet?> GetMovesAsync(Guid gameId, Guid moveSetId)
    {
        var game = await GetGameAsync(gameId);
        return game?.MoveSets.FirstOrDefault(ms => ms.Id == moveSetId);
    }

    public async Task<Guid> UpsertMoveSetAsync(string gameUrl, MoveSet newMoveSet, Guid? previousTurnId)
    {
        ArgumentNullException.ThrowIfNull(gameUrl, nameof(gameUrl));
        ArgumentNullException.ThrowIfNull(newMoveSet, nameof(newMoveSet));

        var game = await GetGameAsync(gameUrl);
        if (game is null)
        {
            throw new ApplicationException($"DataManager couldn't find a game for {gameUrl}.");
        }

        if (previousTurnId is not null)
        {
            newMoveSet.PreviousSet = game.MoveSets.First(ms => ms.Id == previousTurnId);
        }

        var setsForThisTurn = game.MoveSets.Where(ms => ms.Year == newMoveSet.Year && ms.SeasonIndex == newMoveSet.SeasonIndex).OrderBy(ms => ms.FirstSeen).ToList();
        var activeSet = setsForThisTurn.LastOrDefault();

        if (activeSet is not null
            && activeSet.PreviousSet?.Id == newMoveSet.PreviousSet?.Id
            && activeSet.PreRetreatHash == newMoveSet.PreRetreatHash
            && (!activeSet.Satisfied || activeSet.FullHash == newMoveSet.FullHash))
        {
            return await UpdateMoveSetAsync(activeSet, newMoveSet);
        }
        else
        {
            return await InsertNewMoveSetAsync(game, newMoveSet);
        }
    }

    public async Task<(bool success, string? reason)> DeleteGameAsync(Guid gameId, User dbUser)
    {
        var game = await GetGameAsync(gameId);
        if (game is null)
        {
            return (false, "No such game");
        }

        if (game.Owner != dbUser)
        {
            return (false, "User is not authorized to remove this game.");
        }

        appDb.Remove(game);
        await appDb.SaveChangesAsync();
        return (true, null);
    }

    private static (string sanitizedUrl, string foreignId) ExtractKeyValues(string incomingUrl)
    {
        if (string.IsNullOrEmpty(incomingUrl))
        {
            throw new ArgumentNullException(nameof(incomingUrl));
        }

        var urlMatch = UrlSanitizationPattern().Match(incomingUrl);
        if (!urlMatch.Success)
        {
            throw new ApplicationException($"DiploDataManager.ExtractKeyValues: Couldn't get URL from '{incomingUrl}'");
        }

        var sanitizedUrl = urlMatch.Value;
        if (sanitizedUrl.Last() != '/')
        {
            sanitizedUrl += '/';
        }

        var idMatch = ForeignIdPattern().Match(sanitizedUrl);
        if (!idMatch.Success || idMatch.Groups.Count != 2)
        {
            throw new ApplicationException($"DiploDataManager.ExtractKeyValues: Couldn't get ID from '{incomingUrl}'");
        }

        return (sanitizedUrl, idMatch.Groups[1].Value);
    }

    private async Task<Guid> InsertNewMoveSetAsync(Game game, MoveSet newMoveSet)
    {
        newMoveSet.FirstSeen = newMoveSet.LastSeen;
        game.MoveSets.Add(newMoveSet);

        appDb.Update(game);
        await appDb.SaveChangesAsync();

        return newMoveSet.Id;
    }

    private async Task<Guid> UpdateMoveSetAsync(MoveSet oldMoveSet, MoveSet newMoveSet)
    {
        if (oldMoveSet.Orders.GetOrderSetHash() != newMoveSet.Orders.GetOrderSetHash())
        {
            // If we're here, we should only be updating the retreats.
            oldMoveSet.Orders.AddRange(newMoveSet.Orders.OnlyRetreats());
            //oldMoveSet.FullHash = newMoveSet.FullHash;
        }

        oldMoveSet.State = newMoveSet.State;
        oldMoveSet.LastSeen = newMoveSet.LastSeen;

        appDb.Update(oldMoveSet);
        await appDb.SaveChangesAsync();

        return oldMoveSet.Id;
    }
}

public static partial class ServiceExtensions
{
    extension(IServiceCollection services)
    {
        public void AddDiploDataManager()
        {
            services.AddScoped<DiploDataManager>();
        }
    }
}
