using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace GitHubReleaseChecker;

internal class GitHubReleaseService : IGitHubReleaseService
{
    private readonly ILogger<GitHubReleaseService> _logger;
    
    public GitHubReleaseService(ILogger<GitHubReleaseService> logger)
    {
        _logger = logger;
    }

    public async Task<ICollection<GitHubRelease>?> GetReleasesAsync(string owner, string repo, TimeSpan? timeout = null)
    {
        var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/releases";

        string response;

        try
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "GitHubReleaseChecker");
            client.Timeout = timeout ?? TimeSpan.FromSeconds(5);
            response = await client.GetStringAsync(apiUrl);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to call GitHub Release API");
            return null;
        }

        var releases = JsonSerializer.Deserialize<ICollection<GitHubRelease>>(response);

        if (releases == null)
        {
            _logger.LogWarning("Unable to parse GitHub JSON");
        }
        else
        {
            _logger.LogInformation("Retrieved {Count} releases from GitHub", releases?.Count);
        }
        
        return releases;
    }
}