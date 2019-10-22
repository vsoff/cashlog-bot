using System;
using System.Collections.Generic;
using System.Text;

namespace Cashlog.Core.Core
{
    public class TestCashogSettings : ICashogSettings
    {
        public string DataBaseConnectionString => @"Server=localhost;Database=Receipt1;Trusted_Connection=True;";
        public string AdminChatToken => "";
        public string TelegramBotToken => "";
        public string ProxyAddress => "";
        public string FnsPhone => "";
        public string FnsPassword => "";
    }
}