using System.Text.Json.Serialization;

namespace GitHubReleaseChecker;

public class GitHubReleaseAsset
{
    [JsonPropertyName("name")] 
    public string Name { get; set; } = "";

    [JsonPropertyName("browser_download_url")]
    public string Url { get; set; } = "";
}