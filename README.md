# Git Assist 🤖

A Telegram-based GitHub assistant for developers.

Git Assist lets you browse a GitHub account, choose a repository, and quickly inspect repository information, open issues, pull requests, and recent commits — directly from Telegram.

> 🚧 **Status:** Early public preview (`v0.1.0`)
>
> The project is actively being developed, so features and APIs may change.

## ✨ Features

- 👤 Enter a GitHub username
- 📦 Browse public repositories for that owner
- 🔄 Change repository without re-entering the owner
- 👤 Change GitHub owner
- 📊 View repository information
- 🐛 Browse open issues
- 🔀 Browse open pull requests
- 📝 Browse recent commits
- 💬 Telegram reply and inline keyboards for navigation

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

## 🛠 Tech Stack

- C#
- .NET 9
- Telegram.Bot
- GitHub REST API
- `HttpClient`
- `System.Text.Json`
- Dependency Injection
- LINQ
- Async/Await

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
└── README.md
```

## 🚀 Getting Started

### Prerequisites

- .NET 9 SDK
- A Telegram bot token from [@BotFather](https://t.me/BotFather)
- Internet access to Telegram Bot API and GitHub API

### 1. Clone the repository

```bash
git clone https://github.com/MaisDevelover-01/GitAssist.git
cd GitAssist
```

### 2. Set the Telegram bot token

Git Assist reads the token from the `TELEGRAM_BOT_TOKEN` environment variable.

PowerShell:

```powershell
$env:TELEGRAM_BOT_TOKEN="YOUR_BOT_TOKEN"
```

Do **not** commit the bot token to the repository.

### 3. Run the bot

```bash
dotnet restore
dotnet run
```

### 4. Open the bot in Telegram

[Open Git Assist on Telegram](https://t.me/GitAssistDeveloperBot)

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
