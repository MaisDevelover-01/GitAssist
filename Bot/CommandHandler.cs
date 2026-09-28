using GitAssist.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Concurrent;
using GitAssist.Models;
using GitAssist.Bot;
using System.Diagnostics.Eventing.Reader;
namespace GitAssist.bot;

public class CommandHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IGitHubService _gitHubService;
    private readonly ConcurrentDictionary<long, UserSession> _userSession = new();
    public CommandHandler(ITelegramBotClient bot, IGitHubService gitHubService)
    {
        _bot = bot;
        _gitHubService = gitHubService;
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
        if (text == "👤 GitHub Username" || text == "/username")
        {
            await AskForOwnerAsync(message);
            return;
        }
        if (text == "ℹ️ Help" || text == "/help")
        {
            await HandleHelpAsync(message);
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
            await HandleIssuesAsync(message, text);
            return;
        }
        
        if (text == "🔀 Pull Requests" || text == "/prs")
        {
            await HandlePullRequestsAsync(message);
            return;
        }
        if(text == "📝 Commits")
        {
            await HandleCommitsAsync(message);
            return;
        }
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
    private async Task ShowRepositoriesForOwnerAsync(Message message, string owner)
    {
        var repositories = await _gitHubService.GetRepositoriesAsync(owner);

        if (repositories.Count == 0)
        {
            await _bot.SendMessage(message.Chat.Id,
            """
            ❌ I couldn't find any public repositories.

            Please check the GitHub username and try again.
            """
            );
            return;
        }
        var session = GetSession(message.Chat.Id);
        session.PendingOwner = owner;
        session.State = UserState.WaitingForRepository;
        var buttons = new List<List<KeyboardButton>>();
        for (int i = 0; i < repositories.Count; i += 2)
        {
            var row = new List<KeyboardButton>
            {
                new KeyboardButton(repositories[i].Name)
            };
            if (i + 1 < repositories.Count)
            {
                row.Add(new KeyboardButton(repositories[i + 1].Name));
            }
            buttons.Add(row);
        }
        // Change Owner button
        buttons.Add(
        [
            new KeyboardButton("👤 Change Owner")
        ]);
        var keyboard = new ReplyKeyboardMarkup(buttons)
        {
            ResizeKeyboard = true
        };
        await _bot.SendMessage(message.Chat.Id, $"📦 Repositories of {owner}\n\n" +"Choose a repository:",
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
        session.ActiveRepository =
            new ActiveRepository
            {
                Owner = owner,
                Repository = repository
            };
        // Clear temporary state
        session.PendingOwner = null;
        session.State = UserState.None;
         // Show repository information
        await SendRepositoryInfoAsync(message, repo);
    }
    
     // Show repository information
    private async Task SendRepositoryInfoAsync(Message message, RepositoryInfo repo)
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
        await _bot.SendMessage(message.Chat.Id, response);
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
        await _bot.SendMessage(message.Chat.Id, "What would you like to explore? 👇",
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
    private async Task HandleIssuesAsync(Message message, string text)
    {
        if (!TryGetActiveRepository(message.Chat.Id, out var activeRepository))
        {
            await _bot.SendMessage(message.Chat.Id,
            """
                ❌ No repository selected.

                Please choose 📦 Repository first.
            """
            );
            return;
        }
        var issues = await _gitHubService.GetIssueInfosAsync(activeRepository.Owner, activeRepository.Repository);
        var openIssues = issues.Where(issue => issue.PullRequest is null).ToList();
        if (openIssues.Count == 0)
        {
            await _bot.SendMessage(
                message.Chat.Id,
                $"🎉 No open issues found in {activeRepository.Owner}/{activeRepository.Repository}."
            );
            return;
        }
        var messageText = $"🐛 Open Issues — {activeRepository.Owner}/{activeRepository.Repository}\n\n";
        foreach (var issue in openIssues)
        {
            messageText +=$"#{issue.Number} — {issue.Title}\n" +$"{issue.HtmlUrl}\n\n";
        }
        await _bot.SendMessage(message.Chat.Id, messageText);
    }

    // Pull Requests
    private async Task HandlePullRequestsAsync(Message message)
    {
        if (!TryGetActiveRepository(message.Chat.Id, out var activeRepository))
        {
            await _bot.SendMessage(message.Chat.Id,
            """
                ❌ No repository selected.

                Please choose 📦 Repository first.
            """
            );
            return;
        }
        var pullRequests = await _gitHubService.GetPullRequestsAsync(activeRepository.Owner, activeRepository.Repository);
        if (pullRequests.Count == 0)
        {
            await _bot.SendMessage( message.Chat.Id, $"🎉 No open pull requests found in " + 
            $"{activeRepository.Owner}/" + $"{activeRepository.Repository}.");
            return;
        }
        var messageText = $"🔀 Open Pull Requests — " + $"{activeRepository.Owner}/" + $"{activeRepository.Repository}\n\n";
        foreach (var pullRequest in pullRequests)
        {
            var draftText =pullRequest.Draft == true? " 📝 Draft" : string.Empty;
            messageText +=
                $"#{pullRequest.Number} — " +
                $"{pullRequest.Title}" +
                $"{draftText}\n" +
                $"{pullRequest.HtmlUrl}\n\n";
        }
        await _bot.SendMessage( message.Chat.Id, messageText);
    }
    private async Task HandleCommitsAsync(Message message)
    {
        var session = GetSession(message.Chat.Id);
        if(session.ActiveRepository is null)
        {
            await _bot.SendMessage(
            message.Chat.Id,
            """
            ❌ No repository selected.

            Please choose 👤 GitHub Username first.
            """
            );

            return;
        }
        var commits = await _gitHubService.GetCommitsAsync(session.ActiveRepository.Owner, session.ActiveRepository.Repository);
        if(commits.Count == 0)
        {
             await _bot.SendMessage(
            message.Chat.Id,
            $"❌ No commits found in " +
            $"{session.ActiveRepository.Owner}/" +
            $"{session.ActiveRepository.Repository}."
            );

            return;
        }
        var messageText = $"📝 Recent Commits — " +
        $"{session.ActiveRepository.Owner}/" +
        $"{session.ActiveRepository.Repository}\n\n";
        foreach(var commit in commits)
        {
            var shortSha = commit.Sha.Length > 7 ? commit.Sha[..7] : commit.Sha;   
            var commitMessage = commit.Commit.Message.Split("\n")[0];
            var author = commit.Author?.Login?? commit.Commit.Author.Name;
            var date = commit.Commit.Author.Date?.ToLocalTime().ToString("yyyy-MM-dd HH:mm")?? "Unknown date";
            
            messageText +=
            $"🔹 {shortSha}\n" +
            $"📝 {commitMessage}\n" +
            $"👤 {author}\n" +
            $"🕒 {date}\n" +
            $"🔗 {commit.HtmlUrl}\n\n";
        }
        await _bot.SendMessage(message.Chat.Id, messageText);
    }
    private async Task AskForRepositoryAsync(Message message)
    {
        var session = GetSession(message.Chat.Id);
        session.State = UserState.WaitingForRepository;
        var reply = new ForceReplyMarkup
        {
            InputFieldPlaceholder = "owner/repository"
        };
        await _bot.SendMessage(message.Chat.Id,
        """
            📦 Send me the repository.

            Example:

            dotnet/runtime
        """,
            replyMarkup: reply
        );
    }

    // Active Repository
    private bool TryGetActiveRepository(long chatId, out ActiveRepository repository)
    {
        var session = GetSession(chatId);
        if(session.ActiveRepository is null)
        {
            repository = null;
            return false;
        }
        repository = session.ActiveRepository;
        return true;
    }
}