namespace Cashlog.Messenger.Telegram;

public sealed class TelegramOptions
{
    public const string SectionName = nameof(TelegramOptions);
    
    public string AdminChatToken { get; init; }
    public string BotToken { get; init; }
}