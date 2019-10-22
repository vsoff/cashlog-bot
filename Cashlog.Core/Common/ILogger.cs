using System;
using System.Collections.Generic;
using System.Text;

namespace Cashlog.Core.Common
{
    public interface ILogger
    {
        void Info(string text);
        void Trace(string text);
        void Warning(string text);
        void Error(string text, Exception ex = null);
    }

    public class ConsoleLogger : ILogger
    {
        public void Error(string text, Exception ex = null)
            => Console.WriteLine($"{DateTime.UtcNow} UTC [Error]: {text}{(ex == null ? "" : $"\n{ex.Message}: {ex.StackTrace}")}");

        public void Info(string text) => Console.WriteLine($"{DateTime.UtcNow} UTC [Info]: {text}");

        public void Trace(string text) => Console.WriteLine($"{DateTime.UtcNow} UTC [Trace]: {text}");

        public void Warning(string text) => Console.WriteLine($"{DateTime.UtcNow} UTC [Warning]: {text}");
    }
}