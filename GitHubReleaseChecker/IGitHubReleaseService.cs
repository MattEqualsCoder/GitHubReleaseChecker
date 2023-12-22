namespace GitHubReleaseChecker;

/// <summary>
/// Service for retrieving releases from GitHub
/// </summary>
public interface IGitHubReleaseService
{
    /// <summary>
    /// Gets the latest releases from GitHub
    /// </summary>
    /// <param name="owner">The owner of the GitHub repository</param>
    /// <param name="repo">The name of the GitHub repository</param>
    /// <param name="timeout">How long before timing out when calling the Github API (default: 5 seconds)</param> 
    /// <returns></returns>
    public ICollection<GitHubRelease>? GetReleases(string owner, string repo, TimeSpan? timeout = null);
}