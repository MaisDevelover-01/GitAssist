using GitAssist.bot;
using GitAssist.Bot;
using GitAssist.Configuration;
using GitAssist.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using var cts = new CancellationTokenSource();

var builder = Host.CreateApplicationBuilder(args);
var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
builder.Services.AddSingleton(new BotSettings(token));
builder.Services.AddSingleton<TelegramBotClient>(_ => new TelegramBotClient(token));
builder.Services.AddSingleton<TelegramBotClient>(_ => new TelegramBotClient(token, cancellationToken: cts.Token));
builder.Services.AddSingleton<ITelegramBotClient>(services =>services.GetRequiredService<TelegramBotClient>());
builder.Services.AddSingleton<CommandHandler>();
builder.Services.AddSingleton<TelegramBot>();
builder.Services.AddHttpClient<IGitHubService, GitHubService>(
    client =>
    {
        client.BaseAddress = new Uri("https://api.github.com/");
        client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
        client.DefaultRequestHeaders.Add("User-Agent", "Git-Assist");
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10");
    }
);
var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
if (string.IsNullOrWhiteSpace(token))
{
    logger.LogCritical("TELEGRAM_BOT_TOKEN is not set.");
    return;
}
var bot = app.Services.GetRequiredService<TelegramBot>();
await bot.StartAsync();