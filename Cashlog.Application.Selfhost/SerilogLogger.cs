using System;
using Cashlog.Core.Common;
using Serilog.Core;

namespace Cashlog.Application.Selfhost
{
    public class SerilogLogger : ILogger
    {
        private readonly ConsoleLogger _consoleLogger;
        private readonly Logger _logger;

        public SerilogLogger(Logger logger)
        {
            _logger = logger;
            _consoleLogger = new ConsoleLogger();
        }

        public void Info(string text)
        {
            _logger.Information(text);
            _consoleLogger.Info(text);
        }

        public void Trace(string text)
        {
            _logger.Verbose(text);
            _consoleLogger.Trace(text);
        }

        public void Warning(string text)
        {
            _logger.Warning(text);
            _consoleLogger.Warning(text);
        }

        public void Error(string text, Exception ex = null)
        {
            if (ex == null)
                _logger.Error(text);
            else
                _logger.Error(ex, text);

            _consoleLogger.Error(text, ex);
        }
    }
}