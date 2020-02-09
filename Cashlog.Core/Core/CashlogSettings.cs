namespace Cashlog.Core.Core
{
    public class CashlogSettings
    {
        public CashlogSettings(string dataBaseConnectionString, string adminChatToken, string telegramBotToken, string proxyAddress, string fnsPhone, string fnsPassword)
        {
            DataBaseConnectionString = dataBaseConnectionString;
            AdminChatToken = adminChatToken;
            TelegramBotToken = telegramBotToken;
            ProxyAddress = proxyAddress;
            FnsPhone = fnsPhone;
            FnsPassword = fnsPassword;
        }

        public string DataBaseConnectionString { get; }
        public string AdminChatToken { get; }
        public string TelegramBotToken { get; }
        public string ProxyAddress { get; }
        public string FnsPhone { get; }
        public string FnsPassword { get; }
    }
}