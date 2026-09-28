using GitAssist.Models;
namespace GitAssist.Services;

public interface IGitHubService
{
    Task<RepositoryInfo?> GetRepositoryAsync(string owner,string repository);
    Task<List<RepositorySummary>> GetRepositoriesAsync(string owner);
    Task<List<IssueInfo>> GetIssueInfosAsync(string owner, string repository);
    Task<List<PullRequestInfo>> GetPullRequestsAsync(string owner, string repository);
    Task<List<CommitInfo>> GetCommitsAsync(string Owner, string repository);
}