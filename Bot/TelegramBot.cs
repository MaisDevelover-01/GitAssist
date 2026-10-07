using GitAssist.bot;
using GitAssist.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace GitAssist.Bot;

public class TelegramBot
{
    private readonly TelegramBotClient _bot; 
    //The bot Getts a client 
    private readonly CommandHandler _commandHandler; 
    private readonly ILogger<TelegramBot> _logger;
    public TelegramBot(TelegramBotClient bot, CommandHandler commandHandler, ILogger<TelegramBot> logger)
    {
        _bot = bot;
        _commandHandler = commandHandler;
        _logger = logger;
    }
    public async Task StartAsync()
    {
        var me = await _bot.GetMe();//bot is getting its own information from the Telegram API
        Console.WriteLine($"@{me.Username} is running...");
        _bot.OnError += HandleError;
        _bot.OnMessage += HandleMessage;
        _bot.OnUpdate += HandleUpdate;
        //bot is subscribing to the OnMessage event, which is triggered when a new message is received
        Console.WriteLine("Git Assist is running!");
        await Task.Delay(Timeout.Infinite, _bot.GlobalCancelToken);
    }
    private async Task HandleMessage(Message message, UpdateType updateType)
    {
        if(message.Text is null)
            return;
        Console.WriteLine($"Received: {message.Text}");
        try
        {
            await _commandHandler.HandleAsync(message);
        }
        catch (GitHubRateLimitException ex)
        {
            _logger.LogWarning(
                "GitHub rate limit reached. Remaining: {Remaining}, Reset: {Reset}", ex.Remaining, ex.Reset);
                
            await _bot.SendMessage(message.Chat.Id, BuildRateLimitMessage(ex));
        }
        catch (GitHubApiException ex)
        {
            _logger.LogError(ex, "GitHub API error. StatusCode: {StatusCode}", ex.StatusCode);
            await _bot.SendMessage(message.Chat.Id, "❌ GitHub returned an error.\n\nPlease try again later.");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex,"An unexpected error occurred while handling a Telegram message.");

            await _bot.SendMessage(
                message.Chat.Id,
                """
                😕 Something went wrong.

                Please try again.
                """
            );
        }
    }
    private async Task HandleUpdate(Update update)
    {
        if (update.CallbackQuery is { } callbackQuery)
        {
            await _commandHandler.HandleCallbackQueryAsync(callbackQuery);
        }
    }
    private async Task HandleError(Exception exception, Telegram.Bot.Polling.HandleErrorSource source)
    {
        Console.WriteLine("Telegram bot error");
        Console.WriteLine($"Source : {source}");
        Console.WriteLine($"Message : {exception.Message}");
        Console.WriteLine(exception);
        await Task.CompletedTask;
    }
    private string BuildRateLimitMessage(GitHubRateLimitException exception)
    {
        if (long.TryParse(exception.Reset, out var resetUnix))
        {
            var reset = DateTimeOffset.FromUnixTimeSeconds(resetUnix).ToLocalTime();
            var wait = reset - DateTimeOffset.Now;
            if (wait.TotalMinutes > 0)
            {
                return
                    $"⚠️ GitHub API rate limit reached.\n\n" +
                    $"Try again in about {Math.Ceiling(wait.TotalMinutes)} minutes.";
            }
        }
        return "⚠️ GitHub API rate limit reached.\n\n" + "Please try again later.";
    }
}