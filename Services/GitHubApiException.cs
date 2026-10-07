using System.Net;
namespace GitAssist.Services;

public class GitHubApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public GitHubApiException(
        HttpStatusCode statusCode,
        string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}