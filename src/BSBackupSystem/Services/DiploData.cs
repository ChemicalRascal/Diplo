using System.Text.RegularExpressions;
using BSBackupSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace BSBackupSystem.Services;

//public static class AppExtensions
//{
//    extension(IServiceCollection services)
//    {
//        public static void AddDiploDataManager()
//        {
//            services.AddScoped<DiploDataManager>();
//        }
//    }
//}

public partial class DiploDataManager(AppDbContext appDb)
{
    [GeneratedRegex(@"^http.*\.com/sandbox/([^/]*/)?\d{10,}/?")]
    private static partial Regex UrlSanitizationPattern();

    [GeneratedRegex(@"/(\d{10,})/")]
    private static partial Regex ForeignIdPattern();

    public async Task<bool> RegisterGameAsync(string url)
    {
        var (properUrl, foreignId) = ExtractKeyValues(url);

        if (!await appDb.Games.Where(g => g.ForeignId == foreignId).AnyAsync())
        {
            await appDb.Games.AddAsync(new()
            {
                Uri = properUrl,
                ForeignId = foreignId,
                CreationTime = DateTime.UtcNow,
            });
            await appDb.SaveChangesAsync();
            return true;
        }

        return false;
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
}
