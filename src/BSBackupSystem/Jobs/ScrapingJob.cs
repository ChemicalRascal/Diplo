using BSBackupSystem.Data;
using BSBackupSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace BSBackupSystem.Jobs;

public partial class ScrapingJob(AppDbContext appDb, GameReader gameReader, DiploDataManager dataManager)
{
    public async Task Execute()
    {
        Console.WriteLine($"{nameof(ScrapingJob)} started.");

        DateTime creationCutoff= DateTime.UtcNow - TimeSpan.FromDays(5);
        var pendingGames = await appDb.Games.Where(g => g.CreationTime > creationCutoff).ToListAsync();
        var gamesToDelete = await appDb.Games.Where(g => g.CreationTime < creationCutoff).ToListAsync();

        Console.WriteLine($"{pendingGames.Count} games to scrape, {gamesToDelete} to delete.");

        foreach (var game in pendingGames)
        {
            await gameReader.ReadAndPersistGame(game.Uri);
        }
        foreach (var game in gamesToDelete)
        {
            await dataManager.DeleteGameAsync(game.Id);
        }
    }
}

public partial class ScrapingJob : Quartz.IJob
{
    async Task Quartz.IJob.Execute(Quartz.IJobExecutionContext context) => await Execute();
}
