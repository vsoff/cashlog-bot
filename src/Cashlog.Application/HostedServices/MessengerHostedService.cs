using Cashlog.Core.Modules.Messengers;

namespace Cashlog.Application.HostedServices;

public class MessengerHostedService : IHostedService
{
    private readonly IMessenger _messenger;

    public MessengerHostedService(IMessenger messenger)
    {
        _messenger = messenger;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _messenger.StartReceivingAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _messenger.StopReceivingAsync(cancellationToken);
    }
}