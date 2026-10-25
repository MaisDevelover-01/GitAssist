using GitAssist.Bot;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;

public sealed class TelegramBotHostedService : BackgroundService
{
    private readonly TelegramBot _telegramBot;
    private readonly CancellationTokenSource _botCancellation;
    private readonly ILogger<TelegramBotHostedService> _logger;

    private static readonly TimeSpan RetryDelay =
        TimeSpan.FromSeconds(15);

    public TelegramBotHostedService(
        TelegramBot telegramBot,
        CancellationTokenSource botCancellation,
        ILogger<TelegramBotHostedService> logger)
    {
        _telegramBot = telegramBot;
        _botCancellation = botCancellation;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested &&
               !_botCancellation.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation(
                    "Starting Telegram bot...");

                await _telegramBot.StartAsync();

                // StartAsync normally remains active until cancellation.
                break;
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested ||
                      _botCancellation.IsCancellationRequested)
            {
                break;
            }
            catch (RequestException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Telegram API request failed. Retrying in {DelaySeconds} seconds.",
                    RetryDelay.TotalSeconds);
            }

            try
            {
                await Task.Delay(RetryDelay, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation(
            "Telegram bot background service stopped.");
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        _botCancellation.Cancel();

        await base.StopAsync(cancellationToken);
    }
}