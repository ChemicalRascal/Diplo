using BSBackupSystem.Data;
using BSBackupSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace BSBackupSystem.Jobs;

public partial class ScrapingJob(AppDbContext appDb, GameReader gameReader, ILogger<ScrapingJob> logger)
{
    public async Task Execute()
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("{} started", nameof(ScrapingJob));
        }

        DateTime creationCutoff= DateTime.UtcNow - TimeSpan.FromDays(5);
        var pendingGames = await appDb.Games.Where(g => g.CreationTime > creationCutoff).ToListAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("{} games to scrape.", pendingGames.Count);
        }

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
