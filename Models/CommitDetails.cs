using System.Text.Json.Serialization;
namespace GitAssist.Models;

public class CommitDetails
{
    public string Sha { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;
    public CommitDetailsData Commit { get; set; } = new();
    public GitHubUser? Author { get; set; }
    public CommitStats? Stats { get; set; }
    public List<CommitFile> Files { get; set; } = [];
}
public class CommitDetailsData
{
    public string Message { get; set; } = string.Empty;
    public CommitAuthor Author { get; set; } = new();
}

public class CommitStats
{
    public int Total { get; set; }
    public int Additions { get; set; }
    public int Deletions { get; set; }
}

public class CommitFile
{
    public string Filename { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Additions { get; set; }
    public int Deletions { get; set; }
    public int Changes { get; set; }
}