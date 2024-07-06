using Cashlog.Common.Models;
using Cashlog.Messenger.Menu;

namespace Cashlog.Messenger;

public interface IMessenger
{
    ValueTask StartReceivingAsync(CancellationToken cancellationToken);
    ValueTask StopReceivingAsync(CancellationToken cancellationToken);
    ValueTask SendMessageAsync(UserMessageInfo userMessageInfo, string text, bool isReply = false, IMenu menu = null);
    ValueTask EditMessageAsync(UserMessageInfo userMessageInfo, string text, IMenu menu = null);
}
