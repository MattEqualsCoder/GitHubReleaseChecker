using GitHubReleaseChecker;
using Microsoft.Extensions.Logging;
using Moq;

namespace GitHubReleaseCheckerTests;

public class GitHubReleaseCheckerServiceTests
{
    private static List<(string, bool)> _testData = new()
    {
        ("3.2.2-rc.2", true),
        ("3.2.2-rc.1", true),
        ("3.2.1", false),
        ("3.2.1-rc.1", true),
        ("3.2.0", false),
    };
        
    private static List<(string, string?, bool)> _testCases = new List<(string, string?, bool)>()
    {
        ("3.2.1", "", false), // On latest main release
        ("3.2.0", "3.2.1", false), // On prior main release
        ("3.2.1-rc1", "3.2.1", false), // On prior release candidate, should update to next main release
        ("3.2.1-rc1", "3.2.2-rc.2", true), // On prior version's release candidate, should update to next pre-release
        ("3.2.2-rc.1", "3.2.2-rc.2", true), // On prior release candidate version
        ("3.2.2-rc1", "3.2.2-rc.2", true), // On prior release candidate version
        ("3.2.2-rc.2", "", true), // On latest release candidate version
        ("3.2.2-rc.3", "", true), // On newer release candidate version
        ("3.2.3-rc.1", "", true), // On newer release canddiate version
        ("3.2.2", "", false), // On newer full version
        ("4.0.0", "", false), // On newer full version
        ("3.4.0", "", false), // On newer full version
        ("3.2.0", "3.2.1", false), // On older matched version, should update to latest full version
        ("3.2.0", "3.2.2-rc.2", true), // On older matched version, should update to latest release candidate version
        ("3.2.2.2", "", true), // On newer 4 dot version
        ("3.2.2.2", "", false),  // On newer 4 dot version
        ("3.2.1.1", "", false), // On newer 4 dot version
        ("3.2.1.1", "3.2.2-rc.2", true), // On older 4 dot version, update to latest rc
        ("3.2.0.1", "3.2.1", false), // On older 4 dot version, update to latest main release
    };
    
    private static List<(string, bool)> _testData4Dot = new()
    {
        ("1.2.3.2-rc.2", true),
        ("1.2.3.2-rc.1", true),
        ("1.2.3.1", false),
        ("1.2.3", false),
    };
    
    private static List<(string, string?, bool)> _testCases4Dot = new List<(string, string?, bool)>()
    {
        ("1.2.3.1", "", false), // On latest main release
        ("1.2.3", "1.2.3.1", false), // On prior main release
        ("1.2.3.0", "1.2.3.1", false), // On prior main release
        ("1.2.2", "1.2.3.1", false), // On prior main release
        ("1.2", "1.2.3.1", false), // On prior main release
        ("1", "1.2.3.1", false), // On prior main release
        ("1.2.3.2", "", false), // On newer release
        ("1.2.4", "", false), // On newer release
        ("1.3", "", false), // On newer release
        ("2", "", false), // On newer release
    };
    
    private static List<(string, bool)> _testDataSuffix = new()
    {
        ("1.2.3.2-rc.2+abcd1234", true),
        ("1.2.3.2-rc.1+abcd1234", true),
        ("1.2.3.1+abcd1234", false),
        ("1.2.3+abcd1234", false),
    };
    
    private static List<(string, string?, bool)> _testCasesSuffix = new List<(string, string?, bool)>()
    {
        ("1.2.3.1+abce1234", "", false), // On latest main release
        ("1.2.3+abce1234", "1.2.3.1+abcd1234", false), // On prior main release
        ("1.2.3.0+abce1234", "1.2.3.1+abcd1234", false), // On prior main release
        ("1.2.2+abce1234", "1.2.3.1+abcd1234", false), // On prior main release
        ("1.2+abce1234", "1.2.3.1+abcd1234", false), // On prior main release
        ("1+abce1234", "1.2.3.1+abcd1234", false), // On prior main release
        ("1.2.3.2+abce1234", "", false), // On newer release
        ("1.2.4+abce1234", "", false), // On newer release
        ("1.3+abce1234", "", false), // On newer release
        ("2+abce1234", "", false), // On newer release
        
        ("1.2.3.1+abcd1234", "", false), // On latest main release
        ("1.2.3+abcd1234", "1.2.3.1+abcd1234", false), // On prior main release
        ("1.2.3.0+abcd1234", "1.2.3.1+abcd1234", false), // On prior main release
        ("1.2.2+abcd1234", "1.2.3.1+abcd1234", false), // On prior main release
        ("1.2+abcd1234", "1.2.3.1+abcd1234", false), // On prior main release
        ("1+abcd1234", "1.2.3.1+abcd1234", false), // On prior main release
        ("1.2.3.2+abcd1234", "", false), // On newer release
        ("1.2.4+abcd1234", "", false), // On newer release
        ("1.3+abcd1234", "", false), // On newer release
        ("2+abcd1234", "", false), // On newer release
        
    };
    
    private GitHubReleaseCheckerService GetService(List<(string, bool)> staticTestData)
    {
        var mockService = new Mock<IGitHubReleaseService>();

        var testData = new List<GitHubRelease>();
        for (var i = 0; i < staticTestData.Count; i++)
        {
            testData.Add( new GitHubRelease()
            {
                Name = "Regular Version",
                Tag = staticTestData[i].Item1,
                Url = "https://www.google.com",
                Created = DateTime.Now.AddDays(-1 * i),
                Published = DateTime.Now.AddDays(-1 * i),
                PreRelease = staticTestData[i].Item2
            });
        }

        mockService
            .Setup(x => x.GetReleases(It.IsAny<string>(), It.IsAny<string>(), null))
            .Returns(testData);
        
        return new GitHubReleaseCheckerService(Mock.Of<ILogger<GitHubReleaseCheckerService>>(), mockService.Object);
    }
    
    [Test]
    public void TestInvalidVersions()
    {
        var service = GetService(_testData);
        Assert.Throws<InvalidOperationException>(() => service.GetGitHubReleaseToUpdateTo("a", "b", "a", true));
        Assert.Throws<InvalidOperationException>(() => service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3-", true));
        Assert.Throws<InvalidOperationException>(() => service.GetGitHubReleaseToUpdateTo("a", "b", "-1.2.3-", true));
        
        Assert.DoesNotThrow(() => service.GetGitHubReleaseToUpdateTo("a", "b", "1+abcd123123", true));
        Assert.DoesNotThrow(() => service.GetGitHubReleaseToUpdateTo("a", "b", "1.2", true));
        Assert.DoesNotThrow(() => service.GetGitHubReleaseToUpdateTo("a", "b", "1.2-rc.1", true));
        Assert.DoesNotThrow(() => service.GetGitHubReleaseToUpdateTo("a", "b", "1.2-rc.1+abcd123123", true));
        Assert.DoesNotThrow(() => service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3", true));
        Assert.DoesNotThrow(() => service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3-1", true));
        Assert.DoesNotThrow(() => service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3-a", true));
        Assert.DoesNotThrow(() => service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3-rc1", true));
        Assert.DoesNotThrow(() => service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3-rc.1", true));
        Assert.DoesNotThrow(() => service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3.4", true));
        Assert.DoesNotThrow(() => service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3.4-rc.1", true));
        Assert.DoesNotThrow(() => service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3.4+abcd123123", true));
        Assert.DoesNotThrow(() => service.GetGitHubReleaseToUpdateTo("a", "b", "1.2.3.4-rc.1+abcd123123", true));
    }

    

    [Test]
    public void TestCheckVersion()
    {
        GitHubRelease? version;

        var service = GetService(_testData);
        var index = 0;
        foreach (var data in _testCases)
        {
            version = service.GetGitHubReleaseToUpdateTo("a", "b", data.Item1, data.Item3);
            var trackText = data.Item3 ? "pre-release" : "main release";
            if (string.IsNullOrEmpty(data.Item2))
            {
                Assert.That(version, Is.Null, $"Case: {index}: Version {data.Item1} should return null when checking for {trackText} but found tag {version?.Tag}");
            }
            else
            {
                Assert.That(version, Is.Not.Null, $"Case: {index}: Version {data.Item1} should return not null when checking for {trackText}");
                Assert.That(version!.Tag, Is.EqualTo(data.Item2), $"Case: {index}: Version {data.Item1} should return tag {data.Item2} when checking for {trackText} but found tag {version?.Tag}");
            }

            index++;
        }
        
        index = 0;
        var suffix = "+abcd1234";
        foreach (var data in _testCases)
        {
            version = service.GetGitHubReleaseToUpdateTo("a", "b", data.Item1+suffix, data.Item3);
            var trackText = data.Item3 ? "pre-release" : "main release";
            if (string.IsNullOrEmpty(data.Item2))
            {
                Assert.That(version, Is.Null, $"Case: {index}: Version {data.Item1+suffix} should return null when checking for {trackText} but found tag {version?.Tag}");
            }
            else
            {
                Assert.That(version, Is.Not.Null, $"Case: {index}: Version {data.Item1+suffix} should return not null when checking for {trackText}");
                Assert.That(version!.Tag, Is.EqualTo(data.Item2), $"Case: {index}: Version {data.Item1+suffix} should return tag {data.Item2} when checking for {trackText} but found tag {version?.Tag}");
            }

            index++;
        }
    }
    
    [Test]
    public void TestCheckVersion4Dot()
    {
        GitHubRelease? version;
        var service = GetService(_testData4Dot);

        var index = 0;
        foreach (var data in _testCases4Dot)
        {
            version = service.GetGitHubReleaseToUpdateTo("a", "b", data.Item1, data.Item3);
            var trackText = data.Item3 ? "pre-release" : "main release";
            if (string.IsNullOrEmpty(data.Item2))
            {
                Assert.That(version, Is.Null, $"Case: {index}: Version {data.Item1} should return null when checking for {trackText} but found tag {version?.Tag}");
            }
            else
            {
                Assert.That(version, Is.Not.Null, $"Case: {index}: Version {data.Item1} should return not null when checking for {trackText}");
                Assert.That(version!.Tag, Is.EqualTo(data.Item2), $"Case: {index}: Version {data.Item1} should return tag {data.Item2} when checking for {trackText} but found tag {version?.Tag}");
            }

            index++;
        }
        
        index = 0;
        var suffix = "+abcd1234";
        foreach (var data in _testCases4Dot)
        {
            version = service.GetGitHubReleaseToUpdateTo("a", "b", data.Item1+suffix, data.Item3);
            var trackText = data.Item3 ? "pre-release" : "main release";
            if (string.IsNullOrEmpty(data.Item2))
            {
                Assert.That(version, Is.Null, $"Case: {index}: Version {data.Item1+suffix} should return null when checking for {trackText} but found tag {version?.Tag}");
            }
            else
            {
                Assert.That(version, Is.Not.Null, $"Case: {index}: Version {data.Item1+suffix} should return not null when checking for {trackText}");
                Assert.That(version!.Tag, Is.EqualTo(data.Item2), $"Case: {index}: Version {data.Item1+suffix} should return tag {data.Item2} when checking for {trackText} but found tag {version?.Tag}");
            }

            index++;
        }
    }
    
    [Test]
    public void TestCheckVersionSuffix()
    {
        GitHubRelease? version;
        var service = GetService(_testDataSuffix);

        var index = 0;
        foreach (var data in _testCasesSuffix)
        {
            version = service.GetGitHubReleaseToUpdateTo("a", "b", data.Item1, data.Item3);
            var trackText = data.Item3 ? "pre-release" : "main release";
            if (string.IsNullOrEmpty(data.Item2))
            {
                Assert.That(version, Is.Null, $"Case: {index}: Version {data.Item1} should return null when checking for {trackText} but found tag {version?.Tag}");
            }
            else
            {
                Assert.That(version, Is.Not.Null, $"Case: {index}: Version {data.Item1} should return not null when checking for {trackText}");
                Assert.That(version!.Tag, Is.EqualTo(data.Item2), $"Case: {index}: Version {data.Item1} should return tag {data.Item2} when checking for {trackText} but found tag {version?.Tag}");
            }

            index++;
        }
    }

}