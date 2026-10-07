namespace GitAssist.Services;

public class GitHubRateLimitException : Exception
{
    public string? Remaining { get; }
    public string? Reset { get; }
    public GitHubRateLimitException(string? remaining, string? reset) : base("GitHub API rate limit reached.")
    {
        Remaining = remaining;
        Reset = reset;
    }
}