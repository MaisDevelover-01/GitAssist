# GitAssist 🤖

**A Telegram bot for exploring GitHub repositories, issues, pull requests, and commits.**

GitAssist is a Telegram bot built with C# and .NET 9 that helps developers explore public GitHub repositories directly from Telegram.

Users can select a GitHub account, browse its repositories, and inspect repository information, open issues, pull requests, and commits without leaving Telegram.

## ✨ Features

### 📦 Repository Exploration

* Browse public repositories by GitHub username.
* View repository information, including description, programming language, stars, forks, open issues, and default branch.
* Switch between repositories or GitHub accounts.
* Navigate through repository pages.

### 🐛 Issues

* Browse open issues in a repository.
* View issue details, including title, description, author, labels, and timestamps.
* Navigate between issues using inline keyboard buttons.
* Move between pages of results.

### 🔀 Pull Requests

* Browse open pull requests.
* View pull request details, including status, author, branches, changed files, commits, additions, and deletions.
* Open pull requests directly on GitHub.
* Navigate through paginated results.

### 📝 Commits

* Browse recent commits.
* View commit messages, authors, timestamps, and change statistics.
* Inspect changed files and their additions and deletions.
* Open commits directly on GitHub.
* Navigate between pages of commits.

### ⚙️ Reliability and Configuration

* Centralized handling of GitHub API errors.
* GitHub rate-limit error handling.
* Structured logging with `ILogger`.
* Environment-based configuration for the Telegram bot token.
* Docker and Docker Compose support.

## 🛠️ Tech Stack

* **Language:** C#
* **Framework:** .NET 9
* **Bot Platform:** Telegram Bot API
* **External API:** GitHub REST API
* **Networking:** `HttpClient`
* **Dependency Injection:** .NET Generic Host
* **Logging:** `ILogger`
* **Containerization:** Docker and Docker Compose
* **Version Control:** Git and GitHub

## 🚀 Getting Started

### Prerequisites

To run GitAssist locally, you need:

* .NET 9 SDK
* A Telegram bot token
* Internet access to Telegram and the GitHub REST API

To run the project with Docker, you also need Docker Desktop or another working Docker Engine with Docker Compose.

### 1. Clone the Repository

```bash
git clone https://github.com/MaisDevelover-01/GitAssist.git
cd GitAssist
```

### 2. Configure the Telegram Bot Token

Create a Telegram bot using [BotFather](https://t.me/BotFather) and obtain its token.

Set the token as an environment variable.

**PowerShell:**

```powershell
$env:TELEGRAM_BOT_TOKEN = "YOUR_TELEGRAM_BOT_TOKEN"
```

Replace `YOUR_TELEGRAM_BOT_TOKEN` with your actual token.

Never commit your real bot token to GitHub.

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Run the Application

```bash
dotnet run
```

Once the bot starts, open it in Telegram and send `/start`.

## 🐳 Running with Docker Compose

Docker Compose provides a convenient way to build and run GitAssist in a container.

### 1. Configure the Environment

Create a `.env` file in the project root, next to `docker-compose.yml`:

```dotenv
TELEGRAM_BOT_TOKEN=YOUR_TELEGRAM_BOT_TOKEN
```

Replace the placeholder with your actual token.

**Important:** Keep `.env` out of version control. Make sure it is listed in `.gitignore` and `.dockerignore`.

### 2. Build and Start the Container

```bash
docker compose up --build -d
```

### 3. View Logs

```bash
docker compose logs -f gitassist
```

### 4. Stop the Application

```bash
docker compose down
```

Running Docker on your own computer does not provide 24/7 availability. To keep GitAssist available continuously, deploy it to a server that remains online.

## 📖 How to Use

1. Open GitAssist in Telegram.
2. Send `/start`.
3. Choose the GitHub username option.
4. Enter a GitHub username.
5. Select a repository from the inline keyboard.
6. Explore repository information, issues, pull requests, or commits.
7. Open an item to view its details and use the navigation buttons to browse additional results.

Use `/help` to view the bot's available commands and instructions.

## 📁 Project Structure

```text
GitAssist/
├── Bot/
│   ├── CommandHandler.cs
│   ├── TelegramBot.cs
│   ├── UserSession.cs
│   └── UserState.cs
│
├── Configuration/
│   └── BotSettings.cs
│
├── Models/
│   ├── ActiveRepository.cs
│   ├── CommitDetails.cs
│   ├── CommitsInfo.cs
│   ├── IssueInfo.cs
│   ├── PagedResult.cs
│   ├── RepositoryInfo.cs
│   └── RepositorySummery.cs
│
├── Services/
│   ├── GitHubService.cs
│   ├── IGitHubService.cs
│   ├── GitHubApiException.cs
│   └── GitHubRateLimitException.cs
│
├── Program.cs
├── GitAssist.csproj
├── Dockerfile
├── docker-compose.yml
├── .dockerignore
├── .gitignore
└── README.md
```

## ⚠️ Current Limitations

* GitAssist currently focuses on public GitHub repositories.
* GitHub API rate limits may affect requests.
* The bot must be running on a connected host to respond to Telegram messages.
* AI-powered features and automatic repository notifications are not currently implemented.

## 🗺️ Roadmap

Potential future improvements include:

* Improved navigation and repository search.
* Support for additional GitHub resources.
* GitHub authentication and private repository access.
* Repository notifications and background monitoring.
* AI-assisted repository exploration and commit or pull request summaries.
* Further improvements to deployment, monitoring, and reliability.

## 🔐 Security

* Store the Telegram bot token in an environment variable.
* Do not commit `.env` or other files containing secrets.
* Keep credentials out of source code and container images.
* Review configuration and access permissions before deploying the bot.

## 📄 License

No license has been added to this repository yet. All rights remain with the respective rights holders. A license can be added when the project's distribution and usage terms have been decided.

## 👤 Author

Built by **Masoume**.