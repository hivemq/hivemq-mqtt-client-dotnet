namespace HiveMQtt.Test.Helpers;

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

/// <summary>
/// A test logger provider that captures log messages for verification in tests.
/// </summary>
public class TestLoggerProvider : ILoggerProvider
{
#pragma warning disable IDE0055 // Fix formatting - StyleCop SA1000 requires space after 'new'
    private readonly ConcurrentDictionary<string, TestLogger> loggers = new ();
#pragma warning restore IDE0055

    /// <summary>
    /// Gets all log entries captured by all loggers created by this provider.
    /// </summary>
#pragma warning disable IDE0055 // Fix formatting - StyleCop SA1000 requires space after 'new'
    public List<LogEntry> LogEntries { get; } = new ();
#pragma warning restore IDE0055

    /// <summary>
    /// Creates a logger instance.
    /// </summary>
    /// <param name="categoryName">The category name for messages produced by the logger.</param>
    /// <returns>A logger instance.</returns>
    public ILogger CreateLogger(string categoryName) =>
        this.loggers.GetOrAdd(categoryName, name => new TestLogger(name, this.LogEntries));

    /// <summary>
    /// Clears all captured log entries.
    /// </summary>
    public void Clear() => this.LogEntries.Clear();

    /// <summary>
    /// Gets log entries for a specific category.
    /// </summary>
    /// <param name="categoryName">The category name to filter by.</param>
    /// <returns>Log entries for the specified category.</returns>
    public IEnumerable<LogEntry> GetLogEntries(string categoryName) =>
        this.LogEntries.Where(e => e.Category == categoryName);

    /// <summary>
    /// Gets log entries at or above a specific log level.
    /// </summary>
    /// <param name="minLevel">The minimum log level.</param>
    /// <returns>Log entries at or above the specified level.</returns>
    public IEnumerable<LogEntry> GetLogEntries(LogLevel minLevel) =>
        this.LogEntries.Where(e => e.LogLevel >= minLevel);

    /// <summary>
    /// Checks if any log entries contain the specified text.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <returns>True if any log entry contains the text.</returns>
    public bool Contains(string text) =>
        this.LogEntries.Any(e => e.Message.Contains(text, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Disposes the logger provider.
    /// </summary>
    public void Dispose()
    {
        this.loggers.Clear();
        this.LogEntries.Clear();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Represents a single log entry.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Gets the log level.
        /// </summary>
        public LogLevel LogLevel { get; init; }

        /// <summary>
        /// Gets the event ID.
        /// </summary>
        public EventId EventId { get; init; }

        /// <summary>
        /// Gets the log message.
        /// </summary>
        public string Message { get; init; } = string.Empty;

        /// <summary>
        /// Gets the exception, if any.
        /// </summary>
        public Exception? Exception { get; init; }

        /// <summary>
        /// Gets the category name.
        /// </summary>
        public string Category { get; init; } = string.Empty;

        /// <summary>
        /// Gets the formatted message.
        /// </summary>
        public string FormattedMessage { get; init; } = string.Empty;
    }

    private sealed class TestLogger : ILogger
    {
        private readonly string categoryName;
        private readonly List<LogEntry> logEntries;

        public TestLogger(string categoryName, List<LogEntry> logEntries)
        {
            this.categoryName = categoryName;
            this.logEntries = logEntries;
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var message = state?.ToString() ?? string.Empty;
            var formattedMessage = formatter(state, exception);

            this.logEntries.Add(new LogEntry
            {
                LogLevel = logLevel,
                EventId = eventId,
                Message = message,
                Exception = exception,
                Category = this.categoryName,
                FormattedMessage = formattedMessage,
            });
        }
    }
}
