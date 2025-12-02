using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Neuma.Core.Logging
{
    public interface ILogger
    {
        bool IsEnabled(LogLevel level);

        void Log(
            LogLevel level,
            string messageTemplate,
            IReadOnlyDictionary<string, object?>? props = null,
            Exception? exception = null,
            string? category = null,
            string? scene = null,
            string? nodePath = null,
            [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0
        );

        void Trace(string messageTemplate, IReadOnlyDictionary<string, object?>? props = null, string? category = null,
            string? scene = null, string? nodePath = null, [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null, [CallerLineNumber] int line = 0);

        void Debug(string messageTemplate, IReadOnlyDictionary<string, object?>? props = null, string? category = null,
            string? scene = null, string? nodePath = null, [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null, [CallerLineNumber] int line = 0);

        void Info(string messageTemplate, IReadOnlyDictionary<string, object?>? props = null, string? category = null,
            string? scene = null, string? nodePath = null, [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null, [CallerLineNumber] int line = 0);

        void Warn(string messageTemplate, IReadOnlyDictionary<string, object?>? props = null, string? category = null,
            string? scene = null, string? nodePath = null, [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null, [CallerLineNumber] int line = 0);

        void Error(string messageTemplate, IReadOnlyDictionary<string, object?>? props = null, string? category = null,
            string? scene = null, string? nodePath = null, [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null, [CallerLineNumber] int line = 0);

        void Fatal(string messageTemplate, IReadOnlyDictionary<string, object?>? props = null, string? category = null,
            string? scene = null, string? nodePath = null, [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null, [CallerLineNumber] int line = 0);

        void Flush();
    }
}

