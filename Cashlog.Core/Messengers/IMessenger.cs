using System;
using System.Threading.Tasks;
using Cashlog.Core.Core.Models;
using Cashlog.Core.Messengers.Menu;

namespace Cashlog.Core.Messengers
{
    public interface IMessenger
    {
        event EventHandler<UserMessageInfo> OnMessage;
        Task SendMessageAsync(UserMessageInfo userMessageInfo, string text, bool isReply = false, IMenu menu = null);
        Task EditMessageAsync(UserMessageInfo userMessageInfo, string text, IMenu menu = null);
    }
}