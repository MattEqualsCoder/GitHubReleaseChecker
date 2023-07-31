using System.Text.Json.Serialization;

namespace GitHubReleaseChecker;

public class GitHubRelease
{
    [JsonPropertyName("name")] 
    public string Name { get; set; } = "";
        
    [JsonPropertyName("html_url")] 
    public string Url { get; set; } = "";
        
    [JsonPropertyName("tag_name")] 
    public string Tag { get; set; } = "";
        
    [JsonPropertyName("draft")] 
    public bool Draft { get; set; }
        
    [JsonPropertyName("prerelease")] 
    public bool PreRelease { get; set; }
        
    [JsonPropertyName("created_at")] 
    public DateTime Created { get; set; }
        
    [JsonPropertyName("published_at")] 
    public DateTime Published { get; set; }

    [JsonPropertyName("assets")]
    public ICollection<GitHubReleaseAsset> Asset { get; set; } = new List<GitHubReleaseAsset>();
}