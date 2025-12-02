using System.Collections.Generic;

namespace Neuma.Core.Logging
{
    internal sealed class NullLogger : ILogger
    {
        public static readonly NullLogger Instance = new();

        public bool IsEnabled(LogLevel level) => false;

        public void Log(LogLevel level, string messageTemplate,
            IReadOnlyDictionary<string, object?>? props = null, System.Exception? exception = null,
            string? category = null, string? scene = null, string? nodePath = null,
            string? member = null, string? file = null, int line = 0)
        {

        }

        public void Trace(string m, IReadOnlyDictionary<string, object?>? p = null, string? c = null,
            string? s = null, string? n = null, string? member = null, string? file = null, int line = 0)
        {

        }

        public void Debug(string m, IReadOnlyDictionary<string, object?>? p = null, string? c = null,
            string? s = null, string? n = null, string? member = null, string? file = null, int line = 0)
        {

        }

        public void Info(string m, IReadOnlyDictionary<string, object?>? p = null, string? c = null,
            string? s = null, string? n = null, string? member = null, string? file = null, int line = 0)
        {

        }

        public void Warn(string m, IReadOnlyDictionary<string, object?>? p = null, string? c = null,
            string? s = null, string? n = null, string? member = null, string? file = null, int line = 0)
        {

        }

        public void Error(string m, IReadOnlyDictionary<string, object?>? p = null, string? c = null,
            string? s = null, string? n = null, string? member = null, string? file = null, int line = 0)
        {

        }

        public void Fatal(string m, IReadOnlyDictionary<string, object?>? p = null, string? c = null,
            string? s = null, string? n = null, string? member = null, string? file = null, int line = 0)
        {

        }

        public void Flush()
        {

        }
    }
}

