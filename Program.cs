using GitAssist.bot;
using GitAssist.Bot;
using GitAssist.Configuration;
using GitAssist.Services;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);
var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
if (string.IsNullOrWhiteSpace(token))
{
    Console.Error.WriteLine(
        "TELEGRAM_BOT_TOKEN is not set.");

    return;
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
builder.Services.AddSingleton<CancellationTokenSource>();
builder.Services.AddSingleton(new BotSettings(token));

builder.Services.AddSingleton<TelegramBotClient>(services =>
{
    var botCancellation =
        services.GetRequiredService<CancellationTokenSource>();

    return new TelegramBotClient(
        token,
        cancellationToken: botCancellation.Token);
});

builder.Services.AddSingleton<ITelegramBotClient>(
    services => services.GetRequiredService<TelegramBotClient>());

builder.Services.AddSingleton<CommandHandler>();
builder.Services.AddSingleton<TelegramBot>();

builder.Services.AddHttpClient<IGitHubService, GitHubService>(
    client =>
    {
        client.BaseAddress = new Uri("https://api.github.com/");
        client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
        client.DefaultRequestHeaders.Add("User-Agent", "Git-Assist");
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10");
    });

builder.Services.AddHostedService<TelegramBotHostedService>();
var app = builder.Build();
// Health endpoints for Render and monitoring.
app.MapGet("/", () => "GitAssist is running.");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.Logger.LogInformation("GitAssist web server configured on port {Port}", port);

await app.RunAsync();