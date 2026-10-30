using Microsoft.Extensions.Logging;

namespace TodoApp.Tests;

internal sealed class TestLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, EventId EventId, Exception? Exception)> Chamadas { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Chamadas.Add((logLevel, eventId, exception));
    }
}
