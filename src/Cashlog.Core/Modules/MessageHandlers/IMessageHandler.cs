using Cashlog.Core.Models;

namespace Cashlog.Core.Modules.MessageHandlers;

public interface IMessageHandler
{
    public MessageType MessageType { get; }
    public Task HandleAsync(UserMessageInfo userMessageInfo);
}