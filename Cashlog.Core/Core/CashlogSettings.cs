using Cashlog.Data;

namespace Cashlog.Core.Core
{
    public class CashlogSettings
    {
        public CashlogSettings()
        {
        }

        public string DataBaseConnectionString { get; set; }
        public DataProviderType DataProviderType { get; set; }
        public string AdminChatToken { get; set; }
        public string TelegramBotToken { get; set; }
        public string ProxyAddress { get; set; }
        public string FnsPhone { get; set; }
        public string FnsPassword { get; set; }
    }
}