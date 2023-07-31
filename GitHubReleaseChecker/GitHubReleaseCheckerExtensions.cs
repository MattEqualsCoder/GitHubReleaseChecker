using Microsoft.Extensions.DependencyInjection;

namespace GitHubReleaseChecker;

public static class GitHubReleaseCheckerExtensions
{
    public static IServiceCollection AddGitHubReleaseCheckerServices(this IServiceCollection services)
    {
        services.AddTransient<IGitHubReleaseService, GitHubReleaseService>();
        services.AddTransient<IGitHubReleaseCheckerService, GitHubReleaseCheckerService>();
        return services;
    }
}