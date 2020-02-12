namespace Cashlog.Core.Core.Models
{
    public static class UserInfoExtensions
    {
        public static bool IsAdmin(this UserMessageInfo messageInfo) => messageInfo.Group.AdminToken == messageInfo.UserToken;
    }
}