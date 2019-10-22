namespace Cashlog.Core.Core
{
    public interface ICashogSettings
    {
        string DataBaseConnectionString { get; }
        string AdminChatToken { get; }
        string TelegramBotToken { get; }
        string ProxyAddress { get; }
        string FnsPhone { get; }
        string FnsPassword { get; }
    }
}