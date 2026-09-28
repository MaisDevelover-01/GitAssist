using System.Text.Json.Serialization;
namespace GitAssist.Models;

public class CommitInfo
{
    public string Sha { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;
    public CommitData Commit { get; set; } = new();
    public GitHubUser? Author { get; set; }
}

public class CommitData
{
    public string Message { get; set; } = string.Empty;

    public CommitAuthor Author { get; set; } = new();
}

public class CommitAuthor
{
    public string Name { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
}

public class GitHubUser
{
    public string Login { get; set; } = string.Empty;
}