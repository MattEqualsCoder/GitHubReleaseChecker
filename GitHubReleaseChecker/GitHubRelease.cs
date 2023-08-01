using System.Text.Json.Serialization;

namespace GitHubReleaseChecker;

/// <summary>
/// A class representing a release on GitHub
/// </summary>
public class GitHubRelease
{
    /// <summary>
    /// The name of the release
    /// </summary>
    [JsonPropertyName("name")] 
    public string Name { get; set; } = "";
        
    /// <summary>
    /// The url on GitHub to view the release
    /// </summary>
    [JsonPropertyName("html_url")] 
    public string Url { get; set; } = "";
        
    /// <summary>
    /// The tag applied to the release
    /// </summary>
    [JsonPropertyName("tag_name")] 
    public string Tag { get; set; } = "";
        
    /// <summary>
    /// If this is a draft release or not
    /// </summary>
    [JsonPropertyName("draft")] 
    public bool Draft { get; set; }
        
    /// <summary>
    /// If this is a pre-release or not
    /// </summary>
    [JsonPropertyName("prerelease")] 
    public bool PreRelease { get; set; }
        
    /// <summary>
    /// When the release was created
    /// </summary>
    [JsonPropertyName("created_at")] 
    public DateTime Created { get; set; }
        
    /// <summary>
    /// When the release was published
    /// </summary>
    [JsonPropertyName("published_at")] 
    public DateTime Published { get; set; }

    /// <summary>
    /// The list of asset downloads
    /// </summary>
    [JsonPropertyName("assets")]
    public ICollection<GitHubReleaseAsset> Asset { get; set; } = new List<GitHubReleaseAsset>();
}