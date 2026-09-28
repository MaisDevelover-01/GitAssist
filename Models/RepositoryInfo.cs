using System.Text.Json.Serialization;
namespace GitAssist.Models;
public class RepositoryInfo
{
    public string Name{get; set;} = string.Empty;

    [JsonPropertyName("full_name")]
    public string FullName{get; set;} = string.Empty;
    public string? Description { get; set; }

    [JsonPropertyName("stargazers_count")]
    public int Stars { get; set; }

    [JsonPropertyName("forks_count")]
    public int Forks { get; set; }

    [JsonPropertyName("open_issues_count")]
    public int OpenIssues { get; set; }

    [JsonPropertyName("default_branch")]
    public string DefaultBranch { get; set; } = string.Empty;
    public string? Language { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

}