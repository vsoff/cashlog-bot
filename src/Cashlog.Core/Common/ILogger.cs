﻿namespace Cashlog.Core.Common;

public interface ILogger
{
    void Info(string text);
    void Trace(string text);
    void Warning(string text);
    void Error(string text, Exception ex = null);
}