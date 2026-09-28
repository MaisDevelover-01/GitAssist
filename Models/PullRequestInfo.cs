using System.Text.Json.Serialization;

namespace GitAssist.Models;

public class PullRequestInfo
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public bool? Draft { get; set; }
    
    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;
}