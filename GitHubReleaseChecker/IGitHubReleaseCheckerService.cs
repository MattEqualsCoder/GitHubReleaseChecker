namespace GitHubReleaseChecker;

/// <summary>
/// Service for comparing the provided version number with the latest release on GitHub
/// </summary>
public interface IGitHubReleaseCheckerService
{
    /// <summary>
    /// Checks GitHub for the latest releases and sees if the user needs to update
    /// </summary>
    /// <param name="owner">The owner of the GitHub repository</param>
    /// <param name="repo">The repository name to check</param>
    /// <param name="currentVersion">The current installed version</param>
    /// <param name="allowPreRelease">If pre-releases should be allowed</param>
    /// <param name="timeout">How long before timing out when calling the Github API (default: 5 seconds)</param>
    /// <returns>The GitHub release the user should update to. If none is found, it will return null.</returns>
    public Task<GitHubRelease?> GetGitHubReleaseToUpdateToAsync(string owner, string repo, string currentVersion,
        bool allowPreRelease, TimeSpan? timeout = null);

    /// <summary>
    /// Compares two version numbers to see if the current version is out of date
    /// </summary>
    /// <param name="currentVersion">The current installed version</param>
    /// <param name="latestVersion">The version to compare against</param>
    /// <returns>True if current version is out of date, false otherwise</returns>
    public bool IsCurrentVersionOutOfDate(string currentVersion, string latestVersion);
}