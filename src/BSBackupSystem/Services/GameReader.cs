using BSBackupSystem.Data;
using System.Text.RegularExpressions;

namespace BSBackupSystem.Services;

public static class AppExtensions
{
    extension(IServiceCollection services)
    {
        public void AddGameReader()
        {
            services.AddScoped<GameReader>();
        }
    }
}

public partial class GameReader(DiploDataManager dataManager)
{
    // TODO: Factories, DI?

    private readonly BlobExtractor extractor = new();
    private readonly DtoTranslator persister = new(dataManager);

    public async Task ReadAndPersistGame(string gameUrl)
    {
        var blobs = await extractor.GetBlobs(gameUrl);

        Guid? previousTurnId = null;
        foreach (var blob in blobs)
        {
            if (blob is null)
            {
                // TODO: Something more nuanced here?
                continue;
            }

            var orders = BlobDeserializer.GetOrderDtos(blob);
            previousTurnId = await persister.TranslateAndPersistTurn(gameUrl, blob, orders, previousTurnId);
        }
    }

    private partial class YearSeason
    {
        private readonly Match regexMatch;

        [GeneratedRegex(@"(\d+)/(\w+)$")]
        private static partial Regex YearSeasonPattern();

        private static Dictionary<string, int> seasonOrderMap = new()
        {
            { "spring", 1 },
            { "fall", 2 },
            { "winter", 3 },
        };

        public YearSeason(string url)
        {
            regexMatch = YearSeasonPattern().Match(url);
        }

        public int Year => int.Parse(regexMatch.Groups[1].Value);
        public string Season => regexMatch.Groups[2].Value;
        public int SeasonIndex => seasonOrderMap[Season];
        public string Path => regexMatch.Groups[0].Value;
    }
}
