using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Linkly.PosApi.Sdk.UnitTest.Common;

/// <summary>Logger which simply stores log message so they can be searched.</summary>
internal sealed class StoreLogger : ILogger
{
    public ConcurrentQueue<LogMessage> Logs { get; set; } = new();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Logs.Enqueue(new LogMessage
        {
            LogLevel = logLevel,
            EventId = eventId,
            Message = formatter(state, exception),
            Exception = exception?.ToString()
        });
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;
}

public sealed class LogMessage
{
    public LogLevel LogLevel { get; set; }

    public EventId EventId { get; set; }

    public string Message { get; set; } = null!;

    public string? Exception { get; set; }
}