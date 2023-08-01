using Microsoft.Extensions.DependencyInjection;

namespace GitHubReleaseChecker;

/// <summary>
/// Adds the GitHubReleaseChecker services to the service collection
/// </summary>
public static class GitHubReleaseCheckerExtensions
{
    /// <summary>
    /// Adds the GitHubReleaseChecker services to the service collection
    /// </summary>
    /// <param name="services">The service collection to add the services to</param>
    /// <returns></returns>
    public static IServiceCollection AddGitHubReleaseCheckerServices(this IServiceCollection services)
    {
        services.AddTransient<IGitHubReleaseService, GitHubReleaseService>();
        services.AddTransient<IGitHubReleaseCheckerService, GitHubReleaseCheckerService>();
        return services;
    }
}