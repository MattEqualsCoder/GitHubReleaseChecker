using System.Runtime.Serialization;
using GitHubReleaseChecker;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitHubReleaseCheckerTests;

public class GitHubReleaseServiceTests
{
    [Test]
    public void TestGetReleases_Success()
    {
        var logger = Mock.Of<ILogger<GitHubReleaseService>>();

        var service = new GitHubReleaseService(logger);

        var releases = service.GetReleases("MattEqualsCoder", "MSURandomizer");
        
        Assert.That(releases?.Any() == true);

        var latestRelease = releases!.First();
        
        Assert.That(!string.IsNullOrWhiteSpace(latestRelease.Name));
        Assert.That(!string.IsNullOrWhiteSpace(latestRelease.Tag));
        Assert.That(!string.IsNullOrWhiteSpace(latestRelease.Url));
    }
    
    [Test]
    public void TestGetReleases_Invalid()
    {
        var logger = Mock.Of<ILogger<GitHubReleaseService>>();

        var service = new GitHubReleaseService(logger);

        var releases = service.GetReleases("MattEqualsCoder", "InvalidRepo");
        
        Assert.That(releases, Is.Null);
    }
    
    [Test]
    public void TestGetReleases_Empty()
    {
        var logger = Mock.Of<ILogger<GitHubReleaseService>>();

        var service = new GitHubReleaseService(logger);

        var releases = service.GetReleases("MattEqualsCoder", "ALttPMSUShuffler");
        
        Assert.That(releases, Is.Not.Null);
        Assert.That(!releases!.Any());
    }
}