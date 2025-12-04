using AngleSharp;
using AngleSharp.Dom;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;

namespace BSBackupSystem.Services;

public partial class GameReader
{
    private record GamestateBlob(string Path, string TurnState, string UnitLine, string OrderLine);

    private class BlobExtractor
    {
        public async Task<IEnumerable<GamestateBlob?>> GetBlobs(string gameUrl)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(gameUrl, nameof(gameUrl));

            GameUrl = gameUrl;

            var allDetails = (await GetAllPagePaths())
                .ToAsyncEnumerable()
                .Select(async (p, _, _) => await ExtractGamestateStrings(p));

            return allDetails.ToBlockingEnumerable();
        }

        private const string HISTORY_PAGE = "ajax/history";
        private const int GAMESTATE_RETRY_COUNT = 5;

        // TODO: Use a HttpHandler that uses a RateLimiter
        // https://learn.microsoft.com/en-us/dotnet/api/system.threading.ratelimiting.slidingwindowratelimiter
        private static HttpClient client = new(
            new SocketsHttpHandler()
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            });
        private static BrowsingContext context = new(Configuration.Default);

        private string GameUrl { get; set; } = string.Empty;

        private async Task<Stream> GetPageContents(string page)
        {
            var req = await client.GetAsync(GameUrl + page);
            return await req.Content.ReadAsStreamAsync();
        }

        private async Task<IDocument> GetPageDocument(string uri)
        {
            var stream = await GetPageContents(uri);
            return await context.OpenAsync(req => req.Content(stream));
        }

        private async Task<IEnumerable<string>> GetAllPagePaths()
        {
            var historyPage = await GetPageDocument(HISTORY_PAGE);

            // Sure hope you like linq
            var allPaths = historyPage.GetElementsByTagName("td")
                .SelectMany(td => td.GetElementsByTagName("a"))
                .Select(a => a.Attributes["href"]?.Value ?? string.Empty)
                .Where(v => !v.IsNullOrEmpty())
                .Select(url => new YearSeason(url))
                .OrderBy(ys => ys.Year)
                .ThenBy(ys => ys.SeasonIndex)
                .Select(ys => ys.Path);

            return allPaths;
        }

        private async Task<GamestateBlob?> ExtractGamestateStrings(string path)
        {
            var attempts = 1;
            GamestateBlob? result = null;

            do
            {
                var page = await GetPageDocument(path);
                var gamestateScript = page.Scripts
                    .Select(s => s.Text)
                    .Where(s => s.Contains("var orders"))
                    .SelectMany(s => s.Split('\n'));

                var stateLine = gamestateScript.Where(l => l.Contains("var stage = "))
                    .SingleOrDefault()?.Trim();
                var unitLine = gamestateScript.Where(l => l.Contains("var unitsByPlayer = "))
                    .SingleOrDefault()?.Trim();
                var orderLine = gamestateScript.Where(l => l.Contains("var orders = "))
                    .SingleOrDefault()?.Trim();

                if (stateLine is null || unitLine is null || orderLine is null)
                {
                    attempts++;
                    continue;
                }

                var turnState = stateLine[
                    (stateLine.IndexOf('"') + 1)..stateLine.LastIndexOf('"')];

                result = new(path, turnState, unitLine, orderLine);
                break;
            }
            while (attempts < GAMESTATE_RETRY_COUNT);

            return result;
        }

    }
}
