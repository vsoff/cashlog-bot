using Cashlog.Core.Models;

namespace Cashlog.Core.Extensions
{
    public static class UserInfoExtensions
    {
        public static bool IsAdmin(this UserMessageInfo messageInfo) => messageInfo.Group.AdminToken == messageInfo.UserToken;
    }
}