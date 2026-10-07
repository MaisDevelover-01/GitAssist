using System.Text.Json.Serialization;

namespace GitAssist.Models;

public class PullRequestInfo
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string State { get; set; } = string.Empty;
    public bool? Draft { get; set; }
    public bool Merged { get; set; }
    public bool? Mergeable { get; set; }
    public int Comments { get; set; }

    [JsonPropertyName("review_comments")]
    public int ReviewComments { get; set; }
    public int Commits { get; set; }
    public int Additions { get; set; }
    public int Deletions { get; set; }

    [JsonPropertyName("changed_files")]
    public int ChangedFiles { get; set; }
    public PullRequestUser? User { get; set; }
    public PullRequestBranch? Head { get; set; }
    public PullRequestBranch? Base { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("merged_at")]
    public DateTime? MergedAt { get; set; }
    
    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;
}
public class PullRequestUser
{
    public string Login { get; set; } = string.Empty;
}
public class PullRequestBranch
{
    public string Ref { get; set; } = string.Empty;
}