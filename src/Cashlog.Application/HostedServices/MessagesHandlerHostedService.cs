using Cashlog.Core.Modules.MessageHandlers;

namespace Cashlog.Application.HostedServices;

public class MessagesHandlerHostedService : IHostedService
{
    private readonly MessagesMainHandler _messagesMainHandler;

    public MessagesHandlerHostedService(MessagesMainHandler messagesMainHandler)
    {
        _messagesMainHandler = messagesMainHandler;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _messagesMainHandler.Subscribe();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _messagesMainHandler.UnSubscribe();
        return Task.CompletedTask;
    }
}