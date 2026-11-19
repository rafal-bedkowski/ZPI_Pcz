using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace WebStore.Tests.Infrastructure;

// Zbiera wszystkie wpisy loga aplikacji (z każdej kategorii), aby testy mogły sprawdzić ich treść i wyjątki.
public sealed class CapturingLoggerProvider : ILoggerProvider
{
    public sealed record Entry(string Category, LogLevel Level, string Message, Exception? Exception);

    private readonly ConcurrentQueue<Entry> _entries = new();

    public IReadOnlyCollection<Entry> Entries => _entries.ToArray();

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, _entries);

    public void Dispose()
    {
    }

    private sealed class CapturingLogger : ILogger
    {
        private readonly string _category;
        private readonly ConcurrentQueue<Entry> _entries;

        public CapturingLogger(string category, ConcurrentQueue<Entry> entries)
        {
            _category = category;
            _entries = entries;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            _entries.Enqueue(new Entry(_category, logLevel, formatter(state, exception), exception));
    }
}
