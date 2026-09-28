using GitAssist.bot;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace GitAssist.Bot;

public class TelegramBot
{
    private readonly TelegramBotClient _bot; 
    //The bot Getts a client 
    private readonly CommandHandler _commandHandler; 
    public TelegramBot(TelegramBotClient bot, CommandHandler commandHandler)
    {
        _bot = bot;
        _commandHandler = commandHandler;
    }
    public async Task StartAsync()
    {
        var me = await _bot.GetMe();//bot is getting its own information from the Telegram API
        Console.WriteLine($"@{me.Username} is running...");
        _bot.OnMessage += HandleMessage;
        //bot is subscribing to the OnMessage event, which is triggered when a new message is received
        Console.WriteLine("Press Enter to stop the bot.");
        Console.ReadLine();
    }
    private async Task HandleMessage(Message message, UpdateType updateType)
    {
        if(message.Text is null)
        {
            return;
        }
         Console.WriteLine($"Received: {message.Text}");
        await _commandHandler.HandleAsync(message);
    }
}