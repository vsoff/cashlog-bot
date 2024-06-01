using Cashlog.Core.Models;

namespace Cashlog.Core.Modules.MessageHandlers;

public interface IMessagesMainHandler
{
    // TODO: Remove exception exit flow, and return results.
    Task HandleMessage(UserMessageInfo userMessageInfo);
}