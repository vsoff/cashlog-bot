namespace Cashlog.Core.Options;

public sealed class CashlogOptions
{
    public const string SectionName = nameof(CashlogOptions);
    
    public string AdminChatToken { get; init; }
    public string TelegramBotToken { get; init; }
}