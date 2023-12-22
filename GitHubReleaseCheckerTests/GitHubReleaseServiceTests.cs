using System.Runtime.Serialization;
using GitHubReleaseChecker;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitHubReleaseCheckerTests;

public class GitHubReleaseServiceTests
{
    [Test]
    public async Task TestGetReleases_Success()
    {
        var logger = Mock.Of<ILogger<GitHubReleaseService>>();

        var service = new GitHubReleaseService(logger);

        var releases = await service.GetReleasesAsync("MattEqualsCoder", "MSURandomizer");
        
        Assert.That(releases?.Any() == true);

        var latestRelease = releases!.First();
        
        Assert.That(!string.IsNullOrWhiteSpace(latestRelease.Name));
        Assert.That(!string.IsNullOrWhiteSpace(latestRelease.Tag));
        Assert.That(!string.IsNullOrWhiteSpace(latestRelease.Url));
    }
    
    [Test]
    public async Task TestGetReleases_Invalid()
    {
        var logger = Mock.Of<ILogger<GitHubReleaseService>>();

        var service = new GitHubReleaseService(logger);

        var releases = await service.GetReleasesAsync("MattEqualsCoder", "InvalidRepo");
        
        Assert.That(releases, Is.Null);
    }
    
    [Test]
    public async Task TestGetReleases_Empty()
    {
        var logger = Mock.Of<ILogger<GitHubReleaseService>>();

        var service = new GitHubReleaseService(logger);

        var releases = await service.GetReleasesAsync("MattEqualsCoder", "GitHubReleaseChecker");
        
        Assert.That(releases, Is.Not.Null);
        Assert.That(!releases!.Any());
    }
}