using GitAssist.Models;
namespace GitAssist.Services;

public interface IGitHubService
{
    Task<RepositoryInfo?> GetRepositoryAsync(string owner,string repository);
    Task<PagedResult<RepositorySummary>> GetRepositoriesAsync(string owner, int page = 1, int perPage = 10);
    Task<List<IssueInfo>> GetIssuesAsync(string owner, string repository, int page = 1,int pages = 5);
    Task<IssueInfo?> GetIssueAsync(string owner, string repository, int issueNumber);
    Task<PagedResult<PullRequestInfo>> GetPullRequestsAsync(string owner, string repository, int page = 1, int perPage = 5);
    Task<PullRequestInfo?> GetPullRequestAsync(string owner, string repository, int pullNumber);
    Task<PagedResult<CommitInfo>> GetCommitsAsync(string owner, string repository, int page = 1, int perPage = 5);
    Task<CommitDetails?> GetCommitDetailsAsync(string owner, string repository, string sha);
}