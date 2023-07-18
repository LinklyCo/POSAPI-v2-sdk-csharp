using Microsoft.Extensions.Logging;
using System;

namespace Linkly.PosApi.Sdk.DemoPos.Utils;

internal class Logger : ILogger
{
    private Action<string> LogUi { get; }

    public Logger(Action<string> action)
    {
        LogUi = action;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        LogUi($"{logLevel}: {formatter(state, exception)} {exception?.Message}");
    }
}
