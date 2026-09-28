using System.Net;
using System.Net.Http.Json;
using GitAssist.Models;
using Telegram.Bot.Types;
namespace GitAssist.Services;

public class GitHubService : IGitHubService
{
    private readonly HttpClient _httpClient;
    public GitHubService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<RepositoryInfo?> GetRepositoryAsync(string owner,string repository)
    {
        var url = $"repos/{owner}/{repository}";
        var response = await _httpClient.GetAsync(url);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RepositoryInfo>();
    }
    public async Task<List<RepositorySummary>> GetRepositoriesAsync(string owner)
    {
        var url =$"users/{owner}/repos?type=owner&sort=updated&direction=desc&per_page=10";
        var response = await _httpClient.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new List<RepositorySummary>();
        }
        response.EnsureSuccessStatusCode();
        var repositories = await response.Content.ReadFromJsonAsync<List<RepositorySummary>>();
        return repositories ?? new List<RepositorySummary>();
    }
    public async Task<List<IssueInfo>> GetIssueInfosAsync(string owner, string repository)
    {
        var url =$"repos/{owner}/{repository}/issues?state=open&per_page=5";
        // Fetch the first 5 open issues
        var response = await _httpClient.GetAsync(url);
        if(response.StatusCode == HttpStatusCode.NotFound)
        {
            return new List<IssueInfo>();
        }
        response.EnsureSuccessStatusCode();// Ensure the response is successful, otherwise throw an exception
        var issues = await response.Content.ReadFromJsonAsync<List<IssueInfo>>();// Deserialize the JSON response into a list of IssueInfo objects
        return issues?? new List<IssueInfo>();// Return the list of issues, or an empty list if the response was null
    }
    public async Task<List<PullRequestInfo>> GetPullRequestsAsync(string owner, string repository)
    {
        var url = $"repos/{owner}/{repository}/pulls?state=open&per_page=5";
        var response = await _httpClient.GetAsync(url);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new List<PullRequestInfo>();
        }
        response.EnsureSuccessStatusCode();
        var pullRequests = await response.Content.ReadFromJsonAsync<List<PullRequestInfo>>();
        return pullRequests?? new List<PullRequestInfo>();
    }
    public async Task<List<CommitInfo>> GetCommitsAsync(string owner, string repository)
    {
        var url = $"repos/{owner}/{repository}/commits?per_page=5";
        var response = await _httpClient.GetAsync(url);
        if(response.StatusCode == HttpStatusCode.NotFound)
        {
            return new List<CommitInfo>();
        }
        response.EnsureSuccessStatusCode();
        var commist = await response.Content.ReadFromJsonAsync<List<CommitInfo>>();
        return commist?? new List<CommitInfo>();
    }
}