# Git Assist 🤖

A Telegram-based GitHub assistant for developers.

GitAssist is a Telegram bot built with C# and .NET 9 that helps developers explore public GitHub repositories directly from Telegram.

Users can select a GitHub account, browse its repositories, and inspect repository information, open issues, pull requests, and commits without leaving Telegram.

> 🚧 **Status:** Early public preview (`v0.1.0`)
> The project is actively being developed, so features and APIs may change.

## ✨ Features

📦 Repository Exploration
Browse public repositories by GitHub username.
View repository information, including description, programming language, stars, forks, open issues, and default branch.
Switch between repositories or GitHub accounts.
Navigate through repository pages.
🐛 Issues
Browse open issues in a repository.
View issue details, including title, description, author, labels, and timestamps.
Navigate between issues using inline keyboard buttons.
Move between pages of results.
🔀 Pull Requests
Browse open pull requests.
View pull request details, including status, author, branches, changed files, commits, additions, and deletions.
Open pull requests directly on GitHub.
Navigate through paginated results.
📝 Commits
Browse recent commits.
View commit messages, authors, timestamps, and change statistics.
Inspect changed files and their additions and deletions.
Open commits directly on GitHub.
Navigate between pages of commits.

⚙️ Reliability and Configuration
Centralized handling of GitHub API errors.
GitHub rate-limit error handling.
Structured logging with ILogger.
Environment-based configuration for the Telegram bot token.
Docker and Docker Compose support.


## 🎬 Demo

_Add the demo GIF to `assets/demo.gif` before publishing._

![Git Assist demo](assets/demo.gif)

## 🧭 How it works

```text
Telegram User
     ↓
Telegram Bot
     ↓
CommandHandler
     ↓
GitHub Service
     ↓
GitHub REST API
```

The bot keeps the user's current conversation state and selected repository in memory while the application is running.

🛠️ Tech Stack
Language: C#
Framework: .NET 9
Bot Platform: Telegram Bot API
External API: GitHub REST API
Networking: HttpClient
Dependency Injection: .NET Generic Host
Logging: ILogger
Containerization: Docker and Docker Compose
Version Control: Git and GitHub

## 📁 Project Structure

```text
GitAssist/
│
├── Bot/
│   ├── TelegramBot.cs
│   ├── CommandHandler.cs
│   ├── UserState.cs
│   └── UserSession.cs
│
├── Configuration/
│   └── BotSettings.cs
│
├── Models/
│   ├── ActiveRepository.cs
│   ├── RepositoryInfo.cs
│   ├── RepositorySummary.cs
│   ├── IssueInfo.cs
│   ├── PullRequestInfo.cs
│   └── CommitInfo.cs
│
├── Services/
│   ├── IGitHubService.cs
│   └── GitHubService.cs
│
├── Program.cs
|── Dockerfile
└── README.md
```

## 🚀 Getting Started

Prerequisites

To run GitAssist locally, you need:

.NET 9 SDK
A Telegram bot token
Internet access to Telegram and the GitHub REST API

To run the project with Docker, you also need Docker Desktop or another working Docker Engine with Docker Compose.

### 1. Clone the repository

```bash
git clone https://github.com/MaisDevelover-01/GitAssist.git
cd GitAssist
```

###2. Configure the Telegram Bot Token
Create a Telegram bot using BotFather and obtain its token.
Set the token as an environment variable.
PowerShell:

$env:TELEGRAM_BOT_TOKEN = "YOUR_TELEGRAM_BOT_TOKEN"

Replace YOUR_TELEGRAM_BOT_TOKEN with your actual token.
Never commit your real bot token to GitHub.

###3. Restore Dependencies
dotnet restore
###4. Run the Application
dotnet run

Once the bot starts, open it in Telegram and send /start.

## 🧪 Example Flow

```text
/start
   ↓
👤 GitHub Username
   ↓
octocat
   ↓
Choose Repository
   ↓
Hello-World
   ↓
📦 Repository Info
🐛 Issues
🔀 Pull Requests
📝 Commits
```

## 🗺️ Roadmap

### v0.2.0

- GitHub search
- Branch browsing
- Better error and rate-limit handling
- Automated tests

### v1.0.0

- Stable user experience
- GitHub authentication
- Private repository support
- Notifications/background tasks
- AI-assisted developer features
- Production deployment

## 🔐 Security Notes

Git Assist currently uses a Telegram bot token supplied through an environment variable. Never place secrets directly in source code or commit them to Git.

User session data is currently stored in memory and is lost when the application restarts.

## 🤝 Contributing

Issues and pull requests are welcome. If you find a bug or have an idea, open an issue and describe the expected behavior.

## 📄 License

License: _To be decided._

## 👤 Author

Built by **Masoume**.
