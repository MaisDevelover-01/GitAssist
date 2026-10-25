using System.Text.Json.Serialization;
namespace GitAssist.Models;

public class IssueInfo
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? Body { get; set; }
    public IssueUser? User { get; set; }
    public List<IssueLabel> Labels { get; set; } = [];
    public int Comments { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("pull_request")]
    public object? PullRequest { get; set; }
}
public class IssueUser
{
    public string Login { get; set; } = string.Empty;
}
public class IssueLabel
{
    public string Name { get; set; } = string.Empty;
}