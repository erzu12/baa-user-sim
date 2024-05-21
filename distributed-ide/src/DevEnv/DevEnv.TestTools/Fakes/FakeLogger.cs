using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DevEnv.TestTools.Fakes
{
    /// <summary>
    /// A fake implementation of <see cref="ILogger{TCategoryName}"/>, which simply writes log messages to the debug console.
    /// </summary>
    public class FakeLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            Debug.WriteLine($"{typeof(T)} - {logLevel} - {message}");
        }
    }
}
