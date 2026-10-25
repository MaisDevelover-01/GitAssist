using GitAssist.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Concurrent;
using GitAssist.Models;
using GitAssist.Bot;
using System.Diagnostics.Eventing.Reader;
using Microsoft.Extensions.Logging;
namespace GitAssist.bot;

public class CommandHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IGitHubService _gitHubService;
    private readonly ConcurrentDictionary<long, UserSession> _userSession = new();
    private readonly ILogger<CommandHandler> _logger;
    public CommandHandler(ITelegramBotClient bot, IGitHubService gitHubService, ILogger<CommandHandler> logger)
    {
        _bot = bot;
        _gitHubService = gitHubService;
        _logger = logger;
    }
    //If The user has usersession give it if not make one
    private UserSession GetSession(long chatId)
    {
        return _userSession.GetOrAdd(chatId, _ => new UserSession());
    }

     // Main Message Handler
    public async Task HandleAsync(Message message)
    {
        var text = message.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        } 

        //Global Commands
        if(text == "/start")
        {
            await HandleStartAsync(message);
            return;
        }
        if (text == "ℹ️ Help" || text == "/help")
        {
            await HandleHelpAsync(message);
            return;
        }
        //Navigation
        if (text == "👤 GitHub Username" || text == "/username")
        {
            await AskForOwnerAsync(message);
            return;
        }
        if (text == "🔄 Change Repository")
        {
            await HandleChangeRepositoryAsync(message);
            return;
        }
        if (text == "👤 Change Owner")
        {
            await AskForOwnerAsync(message);
            return;
        }
        //State Handling
        var session = GetSession(message.Chat.Id);

        switch (session.State)
        {
            case UserState.WaitingForOwner:
                await ShowRepositoriesForOwnerAsync(message, text);
                return;

            case UserState.WaitingForRepository:
                await HandleRepositorySelectionAsync(message, text);
                return;
        }
        //Repository Actions
        if (text == "🐛 Issues" )
        {
            await HandleIssuesAsync(message.Chat.Id);
            return;
        }
        
        if (text == "🔀 Pull Requests" || text == "/prs")
        {
            await HandlePullRequestsAsync(message.Chat.Id);
            return;
        }
        if(text == "📝 Commits")
        {
            await HandleCommitsAsync(message.Chat.Id);
            return;
        }
        //Unkown
        await _bot.SendMessage(message.Chat.Id, "🤔 I don't know that command yet.");
    }

    //Start
    private async Task HandleStartAsync(Message message)
    {
        // Reset temporary selection state.
        var session = GetSession(message.Chat.Id);
        session.State = UserState.None;
        session.PendingOwner = null;

        var keyboard = new ReplyKeyboardMarkup(
            [
                [
                    new KeyboardButton("👤 GitHub Username"),
                    new KeyboardButton("ℹ️ Help")
                ]
            ])
            {
                ResizeKeyboard = true
            };
        await _bot.SendMessage(message.Chat.Id, 
            """
                👋 Hey! I'm Git Assist.

                Your personal GitHub developer assistant 🤖

                Let's get started!

                Choose an option:
            """,
             replyMarkup: keyboard
        );
    }

    private async Task HandleHelpAsync(Message message)
    {
        await _bot.SendMessage(message.Chat.Id,
         """
            🛠 Git Assist Available Commands:

            /start
            Start the bot.

            /help
            Show this help message.

            /username
            You can Enter your GitHub username here and serlect a reposirory.

            After selecting a repository, you can explore:

            🐛 Issues
            🔀 Pull Requests
            📝 Commits
        """);
    }

    //Ask for GitHub username
    private async Task AskForOwnerAsync(Message message)
    {
        var session = GetSession(message.Chat.Id);
        session.State = UserState.WaitingForOwner;
        // We don't need the old temporary owner anymore.
        session.PendingOwner = null;

        var reply = new ForceReplyMarkup
        {
            InputFieldPlaceholder = "GitHub username"
        };
        await _bot.SendMessage(message.Chat.Id,
            """
                👤 Enter the GitHub username.

                Example:

                octocat
            """,
            replyMarkup: reply
        );
    }
    // Receive GitHub username 
    private async Task ShowRepositoriesForOwnerAsync(Message message, string owner, int page = 1)
    {
        var result = await _gitHubService.GetRepositoriesAsync(owner, page);
        if (result.Items.Count == 0)
        {
            await _bot.SendMessage(message.Chat.Id, page == 1
                    ? $"❌ No repositories found for `{owner}`."
                    : "❌ No more repositories."
            );
            return;
        }
        var session = GetSession(message.Chat.Id);
        session.PendingOwner = owner;
        session.State = UserState.WaitingForRepository;
        var buttons = new List<List<InlineKeyboardButton>>();
        for (int i = 0; i < result.Items.Count; i += 2)
        {
            var row = new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(
                    result.Items[i].Name,
                    $"repo:{result.Items[i].Name}"
                )
            };

            if (i + 1 < result.Items.Count)
            {
                row.Add(InlineKeyboardButton.WithCallbackData(
                        result.Items[i + 1].Name,
                        $"repo:{result.Items[i + 1].Name}"
                    )
                );
            }
            buttons.Add(row);
        }
        var navigation = new List<InlineKeyboardButton>();
        if (page > 1)
        {
            navigation.Add(InlineKeyboardButton.WithCallbackData(
                    "⬅️ Previous",
                    $"repos:page:{page - 1}"
                )
            );
        }
        if (result.HasNext)
        {
            navigation.Add(InlineKeyboardButton.WithCallbackData(
                    "Next ➡️",
                    $"repos:page:{page + 1}"
                )
            );
        }
        if (navigation.Count > 0)
        {
            buttons.Add(navigation);
        }
        buttons.Add(
        [
            InlineKeyboardButton.WithCallbackData(
                "👤 Change Owner",
                "repo:change_owner"
            )
        ]);
        var keyboard = new InlineKeyboardMarkup(buttons);
        await _bot.SendMessage(message.Chat.Id,
            $"📂 Repositories of `{owner}` — Page {page}\n\n" +
            "Select a repository:",
            replyMarkup: keyboard
        );
    }
    // Receive repository selection
    private async Task HandleRepositorySelectionAsync(Message message, string repository)
    {
        var session = GetSession(message.Chat.Id);
        if (string.IsNullOrWhiteSpace(session.PendingOwner))
        {
            await _bot.SendMessage(
            message.Chat.Id,
            """
            ❌ I lost the repository selection.

            Please choose your GitHub username again.
            """
            );
            await AskForOwnerAsync(message);
            return;
        }
        // Get full repository information
        var owner = session.PendingOwner;
        var repo = await _gitHubService.GetRepositoryAsync(owner, repository);
        if (repo is null)
        {
            await _bot.SendMessage( message.Chat.Id,
            "❌ Repository not found."
            );
            return;
        }
        // Save active repository
        session.ActiveRepository = new ActiveRepository
            {
                Owner = owner,
                Repository = repository
            };
        _logger.LogInformation("User {ChatId} selected repository {Owner}/{Repository}",message.Chat.Id, owner, repository);
        // Clear temporary state
        session.PendingOwner = null;
        session.State = UserState.None;
         // Show repository information
        await SendRepositoryInfoAsync(message.Chat.Id, repo);
    }

     // Show repository information
    private async Task SendRepositoryInfoAsync(long chatID, RepositoryInfo repo)
    {
        var description = string.IsNullOrWhiteSpace(repo.Description)? "No description": repo.Description;
        var language = string.IsNullOrWhiteSpace(repo.Language)? "Unknown": repo.Language;
        var response = 
        $"""
            📦 {repo.FullName}

            📝 {description}

            ⭐ Stars: {repo.Stars:N0}
            🍴 Forks: {repo.Forks:N0}
            🐛 Open Issues: {repo.OpenIssues:N0}

            🌿 Default Branch: {repo.DefaultBranch}
            💻 Language: {language}

            🔗 {repo.HtmlUrl}
        """;
        await _bot.SendMessage(chatID, response);
        // Repository actions keyboard
        var repositoryKeyboard =
            new ReplyKeyboardMarkup(
            [
            [
                new KeyboardButton("🐛 Issues"),
                new KeyboardButton("🔀 Pull Requests")
            ],
            [
                new KeyboardButton("📝 Commits"),
                new KeyboardButton("🔄 Change Repository")
            ],
            [
                new KeyboardButton("👤 Change Owner")
            ]
            ])
        {
            ResizeKeyboard = true
        };
        await _bot.SendMessage(chatID, "What would you like to explore? 👇",
        replyMarkup: repositoryKeyboard
        );
    }
    private async Task HandleChangeRepositoryAsync(Message message)
    {
        var session = GetSession(message.Chat.Id);
        if (session.ActiveRepository is null)
        {
            await AskForOwnerAsync(message);
            return;
        }
        await ShowRepositoriesForOwnerAsync(message, session.ActiveRepository.Owner);
    }
    // Issues
    private async Task HandleIssuesAsync(long chatId, int page = 1) 
    {
        var session = GetSession(chatId);
        if (session.ActiveRepository is null)
        {
            await _bot.SendMessage(chatId,
            """
            ❌ No repository selected.

            Please choose 👤 GitHub Username first.
            """
            );
            return;
        }
        var issues = await _gitHubService.GetIssuesAsync(session.ActiveRepository.Owner, session.ActiveRepository.Repository, page);
        var openIssues = issues.Where(issue => issue.PullRequest is null).ToList();
        if (openIssues.Count == 0)
        {
            await _bot.SendMessage(chatId, page == 1?
            $"🎉 No open issues found in " :
            "❌ No more issues."
            );
            return;
        }
        var buttons = new List<List<InlineKeyboardButton>>();
        foreach (var issue in openIssues)
        {
            var title = TruncateText(issue.Title, 40);
            buttons.Add(
            [
                new InlineKeyboardButton($"#{issue.Number} {title}", $"issue:{issue.Number}")
            ]);
        }
        var navigation = new List<InlineKeyboardButton>();
        if (page > 1)
        {
            navigation.Add(InlineKeyboardButton.WithCallbackData(
                    "⬅️ Previous",
                    $"issues:page:{page - 1}"
                )
            );
        }
        if (issues.Count >= 30)
        {
            navigation.Add(
                InlineKeyboardButton.WithCallbackData(
                    "Next ➡️",
                    $"issues:page:{page + 1}"
                )
            );
        }

        if (navigation.Count > 0)
        {
            buttons.Add(navigation);
        }
        var keyboard = new InlineKeyboardMarkup(buttons);
        await _bot.SendMessage(chatId,
        $"🐛 Open Issues — " +
        $"{session.ActiveRepository.Owner}/" +
        $"{session.ActiveRepository.Repository}\n\n" +
        "Choose an issue:",
        replyMarkup: keyboard
        );
    }
    private async Task HandleIssueDetailsAsync(long chatId, int issueNumber)
    {
        var session = GetSession(chatId);
        if (session.ActiveRepository is null)
        {
            await _bot.SendMessage(chatId,
            "❌ No repository selected."
            );
            return;
        }
        var issue =await _gitHubService.GetIssueAsync(session.ActiveRepository.Owner, session.ActiveRepository.Repository, issueNumber);        
        if (issue is null)
        {
            await _bot.SendMessage(chatId,
            "❌ Issue not found.");
            return;
        }
        var author =issue.User?.Login?? "Unknown";
        var labels =issue.Labels.Count > 0? string.Join(", ",issue.Labels.Select(label => label.Name)): "None";

        var body =string.IsNullOrWhiteSpace(issue.Body)? "No description.": TruncateText(issue.Body, 2500);
        var created =issue.CreatedAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm")?? "Unknown";
        var updated =issue.UpdatedAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm")?? "Unknown";
        var text =
            $"""
            🐛 #{issue.Number} — {issue.Title}

            📌 State: {issue.State}
            👤 Author: {author}
            💬 Comments: {issue.Comments}
            🏷 Labels: {labels}

            📅 Created: {created}
            🕒 Updated: {updated}

            📝 Description:

            {body}
            """;

        var keyboard = new InlineKeyboardMarkup(
            [
                [
                    new InlineKeyboardButton("🔗 Open on GitHub",issue.HtmlUrl)
                ],
                [
                    new InlineKeyboardButton( "⬅ Back to Issues", "issues:list")
                ]
            ]
        );
        await _bot.SendMessage(chatId, text, replyMarkup: keyboard);
    }
    // Pull Requests
    private async Task HandlePullRequestsAsync(long chatId, int page = 1)
    {
        var session = GetSession(chatId);
        if (session.ActiveRepository is null)
        {
            await _bot.SendMessage(chatId,
                """
                ❌ No repository selected.

                Please choose 👤 GitHub Username first.
                """
            );
            return;
        }

        var result = await _gitHubService.GetPullRequestsAsync(session.ActiveRepository.Owner, session.ActiveRepository.Repository, page);
        var pullRequests = result.Items;
        if (pullRequests.Count == 0)
        {
            await _bot.SendMessage(chatId, page == 1
                    ? $"🎉 No open pull requests found in {session.ActiveRepository.Owner}/{session.ActiveRepository.Repository}."
                    : "❌ No more pull requests."
            );
            return;
        }
        var buttons = new List<List<InlineKeyboardButton>>();
        foreach (var pullRequest in pullRequests)
        {
            var title = TruncateText(pullRequest.Title, 40);
            buttons.Add(
            [
                InlineKeyboardButton.WithCallbackData(
                    $"#{pullRequest.Number} {title}",
                    $"pr:{pullRequest.Number}"
                )
            ]);
        }
        var navigation = new List<InlineKeyboardButton>();
        if (page > 1)
        {
            navigation.Add(InlineKeyboardButton.WithCallbackData(
                    "⬅️ Previous",
                    $"prs:page:{page - 1}"
                )
            );
        }
        if (result.HasNext)
        {
            navigation.Add(InlineKeyboardButton.WithCallbackData(
                    "Next ➡️",
                    $"prs:page:{page + 1}"
                )
            );
        }
        if (navigation.Count > 0)
        {
            buttons.Add(navigation);
        }
        buttons.Add(
        [
            InlineKeyboardButton.WithCallbackData(
                "◀️ Back",
                "back:repository"
            )
        ]);
        var keyboard = new InlineKeyboardMarkup(buttons);
        await _bot.SendMessage(chatId,
            $"🔀 Pull Requests — " +
            $"{session.ActiveRepository.Owner}/" +
            $"{session.ActiveRepository.Repository}\n\n" +
            "Choose a pull request:",
            replyMarkup: keyboard
        );
    }
    private async Task HandlePullRequestDetailsAsync(long chatId, int pullNumber)
    {
        var session = GetSession(chatId);
        if (session.ActiveRepository is null)
        {
            await _bot.SendMessage(chatId,
            "❌ No repository selected."
            );
            return;
        }
        var activeRepository = session.ActiveRepository;
        var pullRequest =
        await _gitHubService.GetPullRequestAsync(activeRepository.Owner, activeRepository.Repository, pullNumber);
        if (pullRequest is null)
        {
            await _bot.SendMessage(chatId,
            "❌ Pull request not found."
            );
            return;
        }
        var author =pullRequest.User?.Login?? "Unknown";
        var headBranch = pullRequest.Head?.Ref?? "Unknown";
        var baseBranch = pullRequest.Base?.Ref?? "Unknown";
        var status = pullRequest.Merged? "🟣 Merged" : pullRequest.State == "open"? "🟢 Open" : "🔴 Closed";
        var mergeable = pullRequest.Mergeable switch
        {
            true => "✅ Yes",
            false => "❌ No",
            null => "⏳ Unknown",
        };
        var body = string.IsNullOrWhiteSpace(pullRequest.Body) ? "No Description." : TruncateText(pullRequest.Body, 2500);
        var created =pullRequest.CreatedAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "Unknown";
        var updated =pullRequest.UpdatedAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm")?? "Unknown";
        var text = 
            $"""
            🔀 #{pullRequest.Number} — {pullRequest.Title}

            {status}

            👤 Author: {author}

            🌿 Head branch: {headBranch}
            ⬇️ Base branch: {baseBranch}

            📦 Commits: {pullRequest.Commits}
            ➕ Additions: {pullRequest.Additions}
            ➖ Deletions: {pullRequest.Deletions}
            📄 Changed files: {pullRequest.ChangedFiles}

            💬 Comments: {pullRequest.Comments}
            💬 Review comments: {pullRequest.ReviewComments}

            🔧 Mergeable: {mergeable}

            📅 Created: {created}
            🕒 Updated: {updated}

            📝 Description:

            {body}
            """;
        var keyboard = new InlineKeyboardMarkup(
            [
                [
                    new InlineKeyboardButton(
                        "🔗 Open on GitHub",
                        pullRequest.HtmlUrl
                    )
                ],
                [
                    new InlineKeyboardButton(
                        "⬅ Back to Pull Requests",
                        "prs:list"
                    )
                ]
            ]);
        await _bot.SendMessage(chatId, text, replyMarkup: keyboard);
    }   
    private async Task HandleCommitsAsync(long chatId, int page = 1)
    {
        var session = GetSession(chatId);
        if (session.ActiveRepository is null)
        {
            await _bot.SendMessage(chatId,
                """
                ❌ No repository selected.

                Please choose 👤 GitHub Username first.
                """
            );
            return;
        }
        var result = await _gitHubService.GetCommitsAsync(session.ActiveRepository.Owner, session.ActiveRepository.Repository, page);
        var commits = result.Items;
        if (commits.Count == 0)
        {
            await _bot.SendMessage(chatId, page == 1
                    ? $"🎉 No commits found in {session.ActiveRepository.Owner}/{session.ActiveRepository.Repository}."
                    : "❌ No more commits."
            );
            return;
        }
        var buttons = new List<List<InlineKeyboardButton>>();
        foreach (var commit in commits)
        {
            var message = commit.Commit.Message.Split('\n')[0];
            var title = TruncateText(message, 40);
            buttons.Add(
            [
                InlineKeyboardButton.WithCallbackData(
                    title,
                    $"commit:{commit.Sha}"
                )
            ]);
        }
        var navigation = new List<InlineKeyboardButton>();
        if (page > 1)
        {
            navigation.Add(InlineKeyboardButton.WithCallbackData(
                    "⬅️ Previous",
                    $"commits:page:{page - 1}"
                )
            );
        }
        if (result.HasNext)
        {
            navigation.Add(InlineKeyboardButton.WithCallbackData(
                    "Next ➡️",
                    $"commits:page:{page + 1}"
                )
            );
        }
        if (navigation.Count > 0)
        {
            buttons.Add(navigation);
        }
        buttons.Add(
        [
            InlineKeyboardButton.WithCallbackData(
                "◀️ Back",
                "back:repository"
            )
        ]);
        var keyboard = new InlineKeyboardMarkup(buttons);
        await _bot.SendMessage(
            chatId,
            $"📝 Commits — " +
            $"{session.ActiveRepository.Owner}/" +
            $"{session.ActiveRepository.Repository}\n\n" +
            "Choose a commit:",
            replyMarkup: keyboard
        );
    }
    private async Task HandleCommitDetailsAsync(long chatId, string sha)
    {
        var session = GetSession(chatId);
        if (session.ActiveRepository is null)
        {
            await _bot.SendMessage(chatId,
                "❌ No repository selected.");
            return;
        }

        var activeRepository =session.ActiveRepository;
        var commit = await _gitHubService.GetCommitDetailsAsync(activeRepository.Owner, activeRepository.Repository, sha);
        if (commit is null)
        {
            await _bot.SendMessage(chatId,
                "❌ Commit not found."
            );
            return;
        }

        var author = commit.Author?.Login?? commit.Commit.Author.Name;
        var message = commit.Commit.Message.Trim();
        var shortSha = commit.Sha.Length > 7? commit.Sha[..7]: commit.Sha;
        var date = commit.Commit.Author.Date?.ToLocalTime().ToString("yyyy-MM-dd HH:mm")?? "Unknown";
        var additions = commit.Stats?.Additions ?? 0;
        var deletions =  commit.Stats?.Deletions ?? 0;
        var total = commit.Stats?.Total ?? 0;
        var filesText = "None";
        if (commit.Files.Count > 0)
        {
            var fileLines = commit.Files.Take(15).Select(file =>
                        $"{GetFileStatusEmoji(file.Status)} " +
                        $"{file.Filename} " +
                        $"➕{file.Additions} " +
                        $"➖{file.Deletions}");

            filesText =string.Join("\n", fileLines);
        }

        var text =
            $"""
            📝 Commit {shortSha}

            👤 Author: {author}
            🕒 Date: {date}

            📊 Changes:
            ➕ Additions: {additions}
            ➖ Deletions: {deletions}
            📄 Total changes: {total}

            📁 Files:

            {filesText}

            💬 Message:

            {TruncateText(message, 2500)}
            """;

        var keyboard = new InlineKeyboardMarkup(
            [
                [
                    new InlineKeyboardButton("🔗 Open on GitHub", commit.HtmlUrl)
                ],
                [
                    new InlineKeyboardButton("⬅ Back to Commits", "commits:list")
                ]
            ]);

        await _bot.SendMessage(chatId, text, replyMarkup: keyboard);
    }
    public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
    {
        // Stop Telegram's loading indicator.
        await _bot.AnswerCallbackQuery(callbackQuery.Id);
        if (callbackQuery.Data is not { } data)
        {
            return;
        }
        if (callbackQuery.Message is null)
        {
            return;
        }
        var chatId = callbackQuery.Message.Chat.Id;
        // Handle Change Owner button first.
        if (data == "repo:change_owner")
        {
            var session = GetSession(chatId);
            session.PendingOwner = null;

            // Use the same state that handles entering a GitHub username.
            session.State = UserState.WaitingForOwner;

            await _bot.SendMessage(chatId, "👤 Please enter the new GitHub username:");
            return;
        }
        // Handle repository selection.
        if (data.StartsWith("repo:", StringComparison.Ordinal))
        {
            var repository = data["repo:".Length..];

            if (string.IsNullOrWhiteSpace(repository))
            {
                return;
            }
            await HandleRepositorySelectionCallbackAsync(callbackQuery, repository);
            return;
        }
        if (data.StartsWith("repos:page:",StringComparison.Ordinal))
        {
            var pageText = data["repos:page:".Length..];
            if (int.TryParse(pageText, out var page))
            {
                var session = GetSession(callbackQuery.Message!.Chat.Id);
                if (string.IsNullOrWhiteSpace(session.PendingOwner))
                {
                    await _bot.AnswerCallbackQuery(callbackQuery.Id, "Repository selection expired.");
                    return;
                }
                await ShowRepositoriesForOwnerAsync(callbackQuery.Message!, session.PendingOwner, page);
            }
            await _bot.AnswerCallbackQuery(callbackQuery.Id);
            return;
        }
        if (data.StartsWith("issue:", StringComparison.Ordinal))
        {
            var numberText = data["issue:".Length..];
            if (!int.TryParse(numberText, out var issueNumber))
            {
                return;
            }
            await HandleIssueDetailsAsync(chatId, issueNumber);
            return;
        }
        if (data == "issues:list")
        {
            await HandleIssuesAsync(chatId);
            return;
        }
        if (data.StartsWith("issues:page:", StringComparison.Ordinal))
        {
            var pageText = data["issues:page:".Length..];
            if (int.TryParse(pageText, out var page))
            {
                await HandleIssuesAsync(callbackQuery.Message.Chat.Id, page);
            }
            await _bot.AnswerCallbackQuery(callbackQuery.Id);
            return;
        }
        if (data.StartsWith("pr:", StringComparison.Ordinal))
        {
            var numberText = data["pr:".Length..];
            if (int.TryParse(numberText, out var pullNumber))
            {
                await HandlePullRequestDetailsAsync(callbackQuery.Message!.Chat.Id, pullNumber);
            }
            await _bot.AnswerCallbackQuery(callbackQuery.Id);
            return;
        }
        if (data.StartsWith("prs:page:", StringComparison.Ordinal))
        {
            var pageText = data["prs:page:".Length..];
            if (int.TryParse(pageText, out var page))
            {
                await HandlePullRequestsAsync(callbackQuery.Message!.Chat.Id, page);
            }
            await _bot.AnswerCallbackQuery(callbackQuery.Id);
            return;
        }
        if (data.StartsWith("commit:", StringComparison.Ordinal))
        {
            var sha = data["commit:".Length..];
            if (string.IsNullOrWhiteSpace(sha))
                return;

            await HandleCommitDetailsAsync(chatId, sha);
            return;
        }
        if (data.StartsWith("commits:page:",
        StringComparison.Ordinal))
        {
            var pageText = data["commits:page:".Length..];
            if (int.TryParse(pageText, out var page))
            {
                await HandleCommitsAsync(callbackQuery.Message!.Chat.Id, page);
            }
            await _bot.AnswerCallbackQuery(callbackQuery.Id);
            return;
        }
    }
    private async Task HandleRepositorySelectionCallbackAsync(CallbackQuery callbackQuery, string repository)
    {
        var chatId = callbackQuery.Message!.Chat.Id;
        var session = GetSession(chatId);
        if (string.IsNullOrWhiteSpace(session.PendingOwner))
        {
            await _bot.SendMessage(chatId, "Repository selection expired. Please select the owner again.");
            return;
        }
        var owner = session.PendingOwner;
        var repo = await _gitHubService.GetRepositoryAsync(owner, repository);
        if (repo is null)
        {
            await _bot.SendMessage(chatId, "❌ Repository not found.");
            return;
        }
        session.ActiveRepository = new ActiveRepository
        {
            Owner = owner,
            Repository = repository
        };
        session.PendingOwner = null;
        session.State = UserState.None;
        await SendRepositoryInfoAsync(chatId, repo);
    }
    private string TruncateText(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return text;
        return text[..maxLength] + "...";
    }
    private string GetFileStatusEmoji(
    string status)
    {
        return status switch
        {
            "added" => "🟢",
            "modified" => "🟡",
            "removed" => "🔴",
            "renamed" => "🔵",
            _ => "⚪"
        };
    }
}