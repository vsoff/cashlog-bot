using Cashlog.Common.Models;

namespace Cashlog.Core.MessageHandlers;

public interface IMessagesMainHandler
{
    // TODO: Remove exception exit flow, and return results.
    Task HandleMessageAsync(UserMessageInfo userMessageInfo, CancellationToken cancellationToken);
}