using Cashlog.Core.Common;

namespace Cashlog.Core.Logging;

public class ConsoleLogger : ILogger
{
    private readonly object _locker = new();

    public void Error(string text, Exception ex = null)
    {
        lock (_locker)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(
                $"{GetUtcNowShortString()} [Error]: {text}{(ex == null ? "" : $"\n{ex.Message}: {ex.StackTrace}")}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    public void Info(string text)
    {
        lock (_locker)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"{GetUtcNowShortString()} [Info]: {text}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    public void Trace(string text)
    {
        lock (_locker)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{GetUtcNowShortString()} [Trace]: {text}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    public void Warning(string text)
    {
        lock (_locker)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{GetUtcNowShortString()} [Warning]: {text}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    private string GetUtcNowShortString()
    {
        var now = DateTime.UtcNow;
        return $"{now.ToShortDateString()} {now.ToShortTimeString()} UTC";
    }
}