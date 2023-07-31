namespace GitHubReleaseChecker;

public interface IGitHubReleaseService
{
    public ICollection<GitHubRelease>? GetReleases(string owner, string repo);
}