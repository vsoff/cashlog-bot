using Cashlog.Common.Models;

namespace Cashlog.Core.MessageHandlers.Handlers;

internal interface IMessageHandler
{
    public MessageType MessageType { get; }
    public Task HandleAsync(UserMessageInfo userMessageInfo);
}