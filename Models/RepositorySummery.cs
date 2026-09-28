using System.Text.Json.Serialization;

namespace GitAssist.Models;

public class RepositorySummary
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;
}