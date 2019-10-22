using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Cashlog.Core.Core.Models
{
    public class UserMessageInfo
    {
        public Group Group { get; set; }
        public Customer[] Customers { get; set; }
        public string UserName { get; set; }
        public string UserToken { get; set; }
        public MessageInfo Message { get; set; }
        public MessageType MessageType { get; set; }
    }

    public class MessageInfo
    {
        public string Token { get; set; }
        public string Text { get; set; }
        public QrCodeData QrCode { get; set; }
    }

    public enum MessageType
    {
        Unknown = 1,
        Text = 2,
        QrCode = 3,
        Command = 4
    }

    public static class UserInfoExtensions
    {
        public static bool IsAdmin(this UserMessageInfo messageInfo) => messageInfo.Group.AdminToken == messageInfo.UserToken;
    }
}