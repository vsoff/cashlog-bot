namespace Cashlog.Core.Common.Workers;

public interface IWorker : IDisposable
{
    void Start();
    void Stop();
}