using System.Text.Json.Serialization;

namespace GitHubReleaseChecker;

/// <summary>
/// Represents a downloadable asset on GitHub
/// </summary>
public class GitHubReleaseAsset
{
    /// <summary>
    /// The name of the download
    /// </summary>
    [JsonPropertyName("name")] 
    public string Name { get; set; } = "";

    /// <summary>
    /// The url to download the asset
    /// </summary>
    [JsonPropertyName("browser_download_url")]
    public string Url { get; set; } = "";
}