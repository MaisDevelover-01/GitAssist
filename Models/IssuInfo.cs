using System.Text.Json.Serialization;
namespace GitAssist.Models;

public class IssueInfo
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;
    [JsonPropertyName("pull_request")]
    public object? PullRequest{ get; set;}
}