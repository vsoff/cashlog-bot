using Cashlog.Core.Models;
using Cashlog.Core.Modules.Messengers.Menu;

namespace Cashlog.Core.Modules.Messengers;

public interface IMessenger
{
    event EventHandler<UserMessageInfo> OnMessage;
    
    ValueTask StartReceivingAsync(CancellationToken cancellationToken);
    ValueTask StopReceivingAsync(CancellationToken cancellationToken);
    
    ValueTask SendMessageAsync(UserMessageInfo userMessageInfo, string text, bool isReply = false, IMenu menu = null);
    ValueTask EditMessageAsync(UserMessageInfo userMessageInfo, string text, IMenu menu = null);
}