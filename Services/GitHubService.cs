using System.Net;
using System.Net.Http.Json;
using GitAssist.Models;
using Microsoft.Extensions.Logging;
namespace GitAssist.Services;

public class GitHubService : IGitHubService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubService> _logger;
    public GitHubService(HttpClient httpClient, ILogger<GitHubService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    private static void EnsureSuccessfulResponse(HttpResponseMessage response)
    {
        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests)
        {
            var remaining = response.Headers.TryGetValues("X-RateLimit-Remaining",
                    out var remainingValues) ? remainingValues.FirstOrDefault() : null;
            var reset = response.Headers.TryGetValues("X-RateLimit-Reset",
                    out var resetValues) ? resetValues.FirstOrDefault() : null;
            throw new GitHubRateLimitException(remaining, reset);
        }
        response.EnsureSuccessStatusCode();
    }
    private async Task<T?> GetAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("GitHub resource not found: {Url}", url);
            return default;
        }
         try
        {
            EnsureSuccessfulResponse(response);
        }
        catch (GitHubRateLimitException ex)
        {
            _logger.LogWarning("GitHub rate limit reached. URL: {Url}, Remaining: {Remaining}, Reset: {Reset}", 
                url, ex.Remaining, ex.Reset);

            throw;
        }
        catch (GitHubApiException ex)
        {
            _logger.LogError(ex,
                "GitHub API request failed. URL: {Url}, StatusCode: {StatusCode}", url,  ex.StatusCode);

            throw;
        }
        return await response.Content.ReadFromJsonAsync<T>();
    }
    public async Task<RepositoryInfo?> GetRepositoryAsync(string owner,string repository)
    {
        return await GetAsync<RepositoryInfo>($"repos/{owner}/{repository}");
    }
    public async Task<PagedResult<RepositorySummary>> GetRepositoriesAsync(string owner, int page = 1, int perPage = 10)
    {
        return await GetPagedAsync<RepositorySummary>($"users/{owner}/repos?type=owner&sort=updated&direction=desc", page, perPage);
    }
    public async Task<List<IssueInfo>> GetIssuesAsync(string owner, string repository, int page = 1, int pages = 5)
    {
        return await GetAsync<List<IssueInfo>>($"repos/{owner}/{repository}/issues?state=open&per_page=5") ?? [];
    }
    public async Task<IssueInfo?> GetIssueAsync(string owner, string repository, int issueNumber)
    {
        return await GetAsync<IssueInfo>($"repos/{owner}/{repository}/issues/{issueNumber}");
    }
    public async Task<PagedResult<PullRequestInfo>> GetPullRequestsAsync(string owner, string repository, int page = 1, int perPage = 5)
    {
        return await GetPagedAsync<PullRequestInfo>( $"repos/{owner}/{repository}/pulls?state=open", page, perPage);
    }
    public async Task<PullRequestInfo?> GetPullRequestAsync(string owner, string repository, int pullNumber)
    {
        return await GetAsync<PullRequestInfo>($"repos/{owner}/{repository}/pulls/{pullNumber}");
    }
    public async Task<PagedResult<CommitInfo>> GetCommitsAsync(string owner, string repository, int page = 1, int perPage = 5)
    {
        return await GetPagedAsync<CommitInfo>($"repos/{owner}/{repository}/commits", page, perPage);
    }
    public async Task<CommitDetails?> GetCommitDetailsAsync(string owner, string repository, string sha)
    {
       return await GetAsync<CommitDetails>($"repos/{owner}/{repository}/commits/{sha}");
    }
    private async Task<PagedResult<T>> GetPagedAsync<T>(string url, int page, int perPage)
    {
        var separator = url.Contains('?') ? '&' : '?';
        var response = await _httpClient.GetAsync( $"{url}{separator}page={page}&per_page={perPage}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new PagedResult<T>
            {
                Page = page
            };
        }
        EnsureSuccessfulResponse(response);
        var items = await response.Content.ReadFromJsonAsync<List<T>>() ?? [];
        var hasNext = response.Headers.TryGetValues("Link", out var linkValues) &&
            linkValues.Any(link => link.Contains("rel=\"next\"", StringComparison.OrdinalIgnoreCase));
        return new PagedResult<T>
        {
            Items = items,
            HasNext = hasNext,
            Page = page
        };
    }
}