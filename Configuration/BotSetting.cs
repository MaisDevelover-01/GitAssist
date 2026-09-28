namespace GitAssist.Configuration;

public class BotSettings
{
    public string Token { get; }
    public BotSettings(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Bot token is missing.");

        Token = token;
    }
}