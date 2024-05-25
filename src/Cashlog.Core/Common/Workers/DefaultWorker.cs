using System;
using System.Threading;

namespace Cashlog.Core.Common.Workers
{
    public class DefaultWorker : IWorker
    {
        private readonly Action _action;
        private readonly ILogger _logger;
        private readonly Timer _timer;
        private readonly TimeSpan _interval;
        private readonly bool _startImmediately;

        private bool _isBusy;
        private readonly object _locker;

        public DefaultWorker(Action action, TimeSpan interval, ILogger logger, bool startImmediately = true)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _locker = new object();
            TimerCallback tm = DoWork;
            _timer = new Timer(tm, null, Timeout.Infinite, Timeout.Infinite);
            _interval = interval;
            _startImmediately = startImmediately;
        }

        private void DoWork(object o)
        {
            lock (_locker)
            {
                if (_isBusy)
                    return;
                _isBusy = true;
            }

            try
            {
                _action.Invoke();
            }
            catch (Exception ex)
            {
                _logger.Error($"В воркере произошло исключение", ex);
            }
            finally
            {
                _isBusy = false;
            }
        }

        public void Start()
        {
            _timer.Change(_startImmediately ? TimeSpan.Zero : _interval, _interval);
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Dispose();
        }
    }
}