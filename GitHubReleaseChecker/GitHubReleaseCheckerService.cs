using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace GitHubReleaseChecker;

internal class GitHubReleaseCheckerService : IGitHubReleaseCheckerService
{
    private readonly ILogger<GitHubReleaseCheckerService> _logger;
    private readonly IGitHubReleaseService _gitHubReleaseService;
    private static readonly Regex s_validVersionFormat = new(@"^([0-9]+\.?)+(-.+)?(\+[0-9a-f]+)?$");
    private static readonly Regex s_net8VersionEnding = new(@"\+[0-9a-f]+$");
    
    public GitHubReleaseCheckerService(ILogger<GitHubReleaseCheckerService> logger, IGitHubReleaseService gitHubReleaseService)
    {
        _logger = logger;
        _gitHubReleaseService = gitHubReleaseService;
    }

    public GitHubRelease? GetGitHubReleaseToUpdateTo(string owner, string repo, string currentVersion, bool allowPreRelease, TimeSpan? timeout = null)
    {
        if (!IsValidVersion(currentVersion))
        {
            _logger.LogError("Invalid local version format {Version}", currentVersion);
            throw new InvalidOperationException($"Invalid local version format {currentVersion}");
        }

        var releases = _gitHubReleaseService.GetReleases(owner, repo, timeout);
        if (releases == null || !releases.Any())
        {
            _logger.LogWarning("Unable to get GitHub Releases");
            return null;
        }
        
        var currentVersionText = currentVersion;
        var currentRelease = releases.FirstOrDefault(x => x.Tag.EndsWith(currentVersionText) && Math.Abs(x.Tag.Length - currentVersionText.Length) <= 3);

        if (currentRelease == null && currentVersionText.Contains("+"))
        {
            var updatedVersionText = s_net8VersionEnding.Replace(currentVersionText, "");
            currentRelease = releases.FirstOrDefault(x => x.Tag.EndsWith(updatedVersionText) && Math.Abs(x.Tag.Length - updatedVersionText.Length) <= 3);
        }
        
        var latestRelease = allowPreRelease ? releases.First() : releases.FirstOrDefault(x => !x.PreRelease);
        if (latestRelease == null)
        {
            _logger.LogWarning("Unable to find valid release");
            return null;
        }
        
        var latestVersionText = latestRelease.Tag.Replace("v", "");
        if (!IsValidVersion(latestVersionText))
        {
            _logger.LogError("Invalid GitHub release version format {Version}", latestVersionText);
#if DEBUG
            throw new InvalidOperationException($"Invalid GitHub release version format {latestVersionText}");
#endif
        }
        
        if (currentRelease != null)
        {
            if (currentRelease == latestRelease || latestRelease.Published < currentRelease.Published)
            {
                _logger.LogInformation("User version of {Local} matches GitHub release {Release}", currentVersion, latestRelease.Tag);
                return null;
            }
            else if (latestRelease.Published > currentRelease.Published)
            {
                _logger.LogInformation("User version of {Local} is older than GitHub release {Release}", currentVersion, latestRelease.Tag);
                return latestRelease;
            }
        }
        
        if (IsCurrentVersionOutOfDate(currentVersionText, latestVersionText))
        {
            return latestRelease;
        }
        else
        {
            return null;
        }

    }
    
    public bool IsCurrentVersionOutOfDate(string currentVersion, string latestVersion)
    {
        var currentVersionBits = currentVersion.Split("-", 2);
        var latestVersionBits = latestVersion.Split("-", 2);

        // Version text matches above (should have been caught above)
        if (latestVersion == currentVersion)
        {
            _logger.LogInformation("User version of {Local} matches GitHub release {Release}", currentVersion, latestVersion);
            return false;
        }
        // Main version matches, so check the rest
        else if (currentVersionBits[0] == latestVersionBits[0])
        {
            // User is on pre-release and latest is the main release
            if (latestVersionBits.Length == 1 & currentVersionBits.Length > 1)
            {
                _logger.LogInformation("User version of {Local} is older than GitHub release {Release}", currentVersion, latestVersion);
                return true;
            }
            // User is on a newer version
            else if (latestVersionBits.Length > 1 && currentVersionBits.Length == 1)
            {
                _logger.LogInformation("User version of {Local} is newer than GitHub release {Release}", currentVersion, latestVersion);
                return false;
            }
            // Else they both have sub versions
            else
            {
                var latestSubVersion = latestVersionBits[1];
                var currentSubVersion = currentVersionBits[1];

                if (latestSubVersion.Length > 0 && currentSubVersion.Length > 0 &&
                    latestSubVersion[0] != currentSubVersion[0])
                {
                    if (latestSubVersion[0] > currentSubVersion[0])
                    {
                        _logger.LogInformation("User version of {Local} is older than GitHub release {Release}", currentVersion, latestVersion);
                        return true;
                    }
                    else if (latestSubVersion[0] < currentSubVersion[0])
                    {
                        _logger.LogInformation("User version of {Local} is newer than GitHub release {Release}", currentVersion, latestVersion);
                        return false;
                    }
                }
                else
                {
                    Regex net8VersionEnding = new(@"\+[0-9a-f]+$");
                    latestSubVersion = s_net8VersionEnding.Replace(latestSubVersion, "");
                    currentSubVersion = s_net8VersionEnding.Replace(currentSubVersion, "");
                    int.TryParse(string.Concat(latestSubVersion.Where(char.IsDigit)), out var latestSubNumber);
                    int.TryParse(string.Concat(currentSubVersion.Where(char.IsDigit)), out var currentSubNumber);

                    if (latestSubNumber > currentSubNumber)
                    {
                        _logger.LogInformation("User version of {Local} is older than GitHub release {Release}", currentVersion, latestVersion);
                        return true;
                    }
                    else if (latestSubNumber < currentSubNumber)
                    {
                        _logger.LogInformation("User version of {Local} is newer than GitHub release {Release}", currentVersion, latestVersion);
                        return false;
                    }
                    else
                    {
                        _logger.LogInformation("User version of {Local} matches GitHub release {Release}", currentVersion, latestVersion);
                        return false;
                    }
                }
            }
        }
        // Main version doesn't match
        else
        {
            Regex invalidCharacters = new(@"[^0-9\.]");
            var cleanedLatestVersion = s_net8VersionEnding.Replace(latestVersionBits[0], "");
            var cleanedCurrentVersion = s_net8VersionEnding.Replace(currentVersionBits[0], "");
            var latestNumbers = invalidCharacters.Replace(cleanedLatestVersion, "").Split(".").Select(int.Parse).ToList();
            var currentNumbers = invalidCharacters.Replace(cleanedCurrentVersion, "").Split(".").Select(int.Parse).ToList();

            for (var i = 0; i < Math.Min(latestNumbers.Count, currentNumbers.Count); i++)
            {
                if (latestNumbers[i] > currentNumbers[i])
                {
                    _logger.LogInformation("User version of {Local} is older than GitHub release {Release}", currentVersion, latestVersion);
                    return true;
                }
                else if (latestNumbers[i] < currentNumbers[i])
                {
                    _logger.LogInformation("User version of {Local} is newer than GitHub release {Release}", currentVersion, latestVersion);
                    return false;
                }
            }

            if (latestNumbers.Count > currentNumbers.Count)
            {
                _logger.LogInformation("User version of {Local} is newer than GitHub release {Release}", currentVersion, latestVersion);
                return true;
            }
            else if (latestNumbers.Count < currentNumbers.Count)
            {
                _logger.LogInformation("User version of {Local} is older than GitHub release {Release}", currentVersion, latestVersion);
                return false;
            }
        }
        
        _logger.LogInformation("Unable to compare user version of {Local} with GitHub release {Release}", currentVersion, latestVersion);
        return false;
    }

    private bool IsValidVersion(string version)
    {
        return s_validVersionFormat.IsMatch(version);
    }
}