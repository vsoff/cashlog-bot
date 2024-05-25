namespace Cashlog.Core.Common.Workers;

public class DefaultWorkerController : IWorkerController
{
    private readonly ILogger _logger;

    public DefaultWorkerController(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IWorker CreateWorker(Action action, TimeSpan interval, bool startImmediately = true)
    {
        return new DefaultWorker(action, interval, _logger, startImmediately);
    }

    public IWorker StartWorker(Action action, TimeSpan interval, bool startImmediately = true)
    {
        var worker = CreateWorker(action, interval, startImmediately);
        worker.Start();
        return worker;
    }
}