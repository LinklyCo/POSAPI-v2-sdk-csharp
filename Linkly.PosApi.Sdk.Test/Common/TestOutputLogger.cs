using System.Text;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Linkly.PosApi.Sdk.UnitTest.Common;

/// <summary>Logger which writes to <see cref="ITestOutputHelper" /> to enable Xunit logging.</summary>
internal sealed class TestOutputLogger : ILogger
{
    private readonly string _name;
    private readonly ITestOutputHelper _testOutput;

    public TestOutputLogger(string name, ITestOutputHelper testOutput)
    {
        _name = name;
        _testOutput = testOutput;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{DateTime.Now:O} {_name} [{logLevel}]: {formatter(state, exception)}");

        if (exception is not null)
            sb.Append($" ({exception})");

        _testOutput.WriteLine(sb.ToString());
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;
}