using System;
using System.Threading.Tasks;
using Cashlog.Core.Models;
using Cashlog.Core.Modules.Messengers.Menu;

namespace Cashlog.Core.Modules.Messengers
{
    public interface IMessenger
    {
        void StartReceiving();
        void StopReceiving();
        event EventHandler<UserMessageInfo> OnMessage;
        Task SendMessageAsync(UserMessageInfo userMessageInfo, string text, bool isReply = false, IMenu menu = null);
        Task EditMessageAsync(UserMessageInfo userMessageInfo, string text, IMenu menu = null);
    }
}