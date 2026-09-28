using System.Data;
using GitAssist.bot;
using GitAssist.Bot;
using GitAssist.Configuration;
using GitAssist.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);
var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
if (string.IsNullOrWhiteSpace(token))
{
    Console.WriteLine("TELEGRAM_BOT_TOKEN is not set.");
    return;
}
builder.Services.AddSingleton(new BotSettings(token));
builder.Services.AddSingleton<TelegramBotClient>(_ => new TelegramBotClient(token));
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

using var host = builder.Build();
var bot = host.Services.GetRequiredService<TelegramBot>();
await bot.StartAsync();