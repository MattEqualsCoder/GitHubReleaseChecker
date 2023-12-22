# GitHub Release Checker

Small Nuget package for getting the finding the latest release on GitHub and comparing a provided version with.

1. Version checking is based on [Semantic Versioning](https://semver.org)
    - It should also work with flexible dot number notations (e.g., 1.2.3.1 will record as newer than 1.2.3 as well as 1.2.3.0)
2. GitHub tags need to match the version (v is removed from the beginning)
3. Only the most recent 10 releases are checked
4. Built to be used with Dependency Injection
5. It should be compatible with the .net build versioning that includes the git commit
    - Note that if the GitHub tags don't include the git commit in them, then it will treat 1.2.3+hash1 as equal to 1.2.3+hash2

# Examples

## Adding the services to a project
```csharp
Host.CreateDefaultBuilder(e.Args)
.ConfigureLogging(...)
.ConfigureServices(services =>
{
    services.AddGitHubReleaseCheckerServices();
})
.Start();
```

## Checking the release 
```csharp
var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

var newerGitHubRelease = _host.Services.GetRequiredService<IGitHubReleaseCheckerService>()
    .GetGitHubReleaseToUpdateTo("MattEqualsCoder", "TestRepo", version ?? "", false);

if (newerGitHubRelease != null)
{
    var response = MessageBox.Show("A new update was found! Do you want to go to the release page?", "New Update",
        MessageBoxButton.YesNo);

    if (response == MessageBoxResult.Yes)
    {
        var url = newerGitHubRelease.Url.Replace("&", "^&");
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
```