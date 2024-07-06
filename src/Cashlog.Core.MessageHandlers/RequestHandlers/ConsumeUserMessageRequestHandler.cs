using Cashlog.Core.MessageHandlers.Handlers;
using Cashlog.Messenger;
using MediatR;

namespace Cashlog.Core.MessageHandlers.RequestHandlers;

public class ConsumeUserMessageRequestHandler : IRequestHandler<ConsumeUserMessageRequest, Unit>
{
    private readonly IMessagesMainHandler _messagesMainHandler;

    public ConsumeUserMessageRequestHandler(
        IMessagesMainHandler messagesMainHandler)
    {
        _messagesMainHandler = messagesMainHandler;
    }

    public async Task<Unit> Handle(ConsumeUserMessageRequest request, CancellationToken cancellationToken)
    {
        await _messagesMainHandler.HandleMessageAsync(request, cancellationToken);
        return Unit.Value;
    }
}