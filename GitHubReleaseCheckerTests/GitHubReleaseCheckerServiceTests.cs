using GitHubReleaseChecker;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitHubReleaseCheckerTests;

public class GitHubReleaseCheckerServiceTests
{
    private GitHubReleaseCheckerService _service = null!;

    [SetUp]
    public void Setup()
    {
        var mockService = new Mock<IGitHubReleaseService>();
        
        mockService
            .Setup(x => x.GetReleases(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new List<GitHubRelease>()
            {
                new GitHubRelease()
                {
                    Name = "Regular Version",
                    Tag = "3.2.2-rc.2",
                    Url = "https://www.google.com",
                    Created = DateTime.Now + TimeSpan.FromDays(1),
                    Published = DateTime.Now + TimeSpan.FromDays(1),
                    PreRelease = true
                },
                new GitHubRelease()
                {
                    Name = "Regular Version",
                    Tag = "3.2.2-rc.1",
                    Url = "https://www.google.com",
                    Created = DateTime.Now,
                    Published = DateTime.Now,
                    PreRelease = true
                },
                new GitHubRelease()
                {
                    Name = "Regular Version",
                    Tag = "3.2.1",
                    Url = "https://www.google.com",
                    Created = DateTime.Now,
                    Published = DateTime.Now
                },
                new GitHubRelease()
                {
                    Name = "PreRelease Version",
                    Tag = "3.2.1-rc.1",
                    Url = "https://www.google.com",
                    Created = DateTime.Today,
                    Published = DateTime.Today,
                    PreRelease = true
                },
                new GitHubRelease()
                {
                    Name = "Regular Version",
                    Tag = "3.2.0",
                    Url = "https://www.google.com",
                    Created = DateTime.Today,
                    Published = DateTime.Today
                },
            });
        
        _service = new GitHubReleaseCheckerService(Mock.Of<ILogger<GitHubReleaseCheckerService>>(), mockService.Object);
    }
    
    [Test]
    public void TestInvalidVersions()
    {
        Assert.Throws<InvalidOperationException>(() => _service.GetGitHubReleaseToUpdateTo("a", "b", "a", true));
        Assert.Throws<InvalidOperationException>(() => _service.GetGitHubReleaseToUpdateTo("a", "b", "1.2", true));
        Assert.Throws<InvalidOperationException>(() => _service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3-", true));
        Assert.Throws<InvalidOperationException>(() => _service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3--", true));
        Assert.Throws<InvalidOperationException>(() => _service.GetGitHubReleaseToUpdateTo("a", "b", "-1.2.3-", true));
        Assert.Throws<InvalidOperationException>(() => _service.GetGitHubReleaseToUpdateTo("a", "b", "1.2-rc.1", true));
        
        Assert.DoesNotThrow(() => _service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3", true));
        Assert.DoesNotThrow(() => _service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3-1", true));
        Assert.DoesNotThrow(() => _service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3-a", true));
        Assert.DoesNotThrow(() => _service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3-rc1", true));
        Assert.DoesNotThrow(() => _service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3-rc.1", true));
    }

    [Test]
    public void TestCheckVersion()
    {
        GitHubRelease? version = null; 
            
        // On latest main release
        version = _service.GetGitHubReleaseToUpdateTo("a", "b", "3.2.1", false);
        Assert.That(version, Is.Null);
        
        // On prior main release
        version = _service.GetGitHubReleaseToUpdateTo("a", "b", "3.2.0", false);
        Assert.That(version, Is.Not.Null);
        Assert.That(version!.Tag, Is.EqualTo("3.2.1"));

        // On prior release candidate, should update to next main release
        version = _service.GetGitHubReleaseToUpdateTo("a", "b", "3.2.1-rc1", false);
        Assert.That(version, Is.Not.Null);
        Assert.That(version!.Tag, Is.EqualTo("3.2.1"));

        // On prior version's release candidate, should update to next pre-release
        version = _service.GetGitHubReleaseToUpdateTo("a", "b", "3.2.1-rc1", true);
        Assert.That(version, Is.Not.Null);
        Assert.That(version!.Tag, Is.EqualTo("3.2.2-rc.2"));

        // On prior release candidate version
        version = _service.GetGitHubReleaseToUpdateTo("a", "b", "3.2.2-rc.1", true);
        Assert.That(version, Is.Not.Null);
        Assert.That(version!.Tag, Is.EqualTo("3.2.2-rc.2"));
        
        // On prior release candidate version
        version = _service.GetGitHubReleaseToUpdateTo("a", "b", "3.2.2-rc1", true);
        Assert.That(version, Is.Not.Null);
        Assert.That(version!.Tag, Is.EqualTo("3.2.2-rc.2"));
        
        // On latest release candiate version
        version = _service.GetGitHubReleaseToUpdateTo("a", "b", "3.2.2-rc.2", true);
        Assert.That(version, Is.Null);
        
        // On newer release candidate version
        version = _service.GetGitHubReleaseToUpdateTo("a", "b", "3.2.2-rc.3", true);
        Assert.That(version, Is.Null);
        
        // On newer release canddiate version
        version = _service.GetGitHubReleaseToUpdateTo("a", "b", "3.2.3-rc.1", true);
        Assert.That(version, Is.Null);
        
        // On newer full version
        version = _service.GetGitHubReleaseToUpdateTo("a", "b", "3.2.2", false);
        Assert.That(version, Is.Null);

        // On newer full version
        version = _service.GetGitHubReleaseToUpdateTo("a", "b", "4.0.0", false);
        Assert.That(version, Is.Null);
        
        // On newer full version
        version = _service.GetGitHubReleaseToUpdateTo("a", "b", "3.4.0", false);
        Assert.That(version, Is.Null);

        // On older matched version, should update to latest full version
        version = _service.GetGitHubReleaseToUpdateTo("a", "b", "3.2.0", false);
        Assert.That(version, Is.Not.Null);
        Assert.That(version!.Tag, Is.EqualTo("3.2.1"));
        
        // On older matched version, should update to latest release candidate version
        version = _service.GetGitHubReleaseToUpdateTo("a", "b", "3.2.0", true);
        Assert.That(version, Is.Not.Null);
        Assert.That(version!.Tag, Is.EqualTo("3.2.2-rc.2"));
    }
}