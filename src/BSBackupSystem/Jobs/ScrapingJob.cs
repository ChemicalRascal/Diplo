using BSBackupSystem.Data;
using BSBackupSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace BSBackupSystem.Jobs;

public partial class ScrapingJob(AppDbContext appDb, GameReader gameReader)
{
    public async Task Execute()
    {
        Console.WriteLine($"{nameof(ScrapingJob)} started.");

        DateTime creationCutoff= DateTime.UtcNow - TimeSpan.FromDays(1);
        var pendingGames = await appDb.Games.Where(g => g.CreationTime > creationCutoff).ToListAsync();

        Console.WriteLine($"{pendingGames.Count} games to scrape.");

        foreach (var game in pendingGames)
        {
            await gameReader.ReadAndPersistGame(game.Uri);
        }
    }
}

public partial class ScrapingJob : Quartz.IJob
{
    async Task Quartz.IJob.Execute(Quartz.IJobExecutionContext context) => await Execute();
}
