# GitHub Release Checker

Small Nuget package for getting the finding the latest release on GitHub and comparing a provided version with.

1. Version checking is based on [Semantic Versioning](https://semver.org)
2. GitHub tags need to match the version (v is removed from the beginning)
3. Only the most recent 10 releases are checked
4. Built to be used with Dependency Injection

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