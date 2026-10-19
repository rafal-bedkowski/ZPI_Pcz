using Microsoft.Extensions.Logging;

namespace WebStore.Tests.Infrastructure;

// Zapamiętuje wpisy loga, aby testy mogły sprawdzić, że błąd usługi został zapisany.
public sealed class TestLogger : ILogger
{
    public List<(LogLevel Level, Exception? Exception)> Entries { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter) => Entries.Add((logLevel, exception));

    public bool HasError<TException>() where TException : Exception =>
        Entries.Any(e => e.Level == LogLevel.Error && e.Exception is TException);
}
