using Cashlog.Common.Models;
using MediatR;

namespace Cashlog.Messenger;

public class ConsumeUserMessageRequest : UserMessageInfo, IRequest<Unit>
{
    public byte[]? PhotoBytes { get; set; }
    public required string ChatToken { get; init; }
}