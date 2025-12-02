using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Neuma.Core.Logging
{
    /// <summary>
    /// Global facade over ILogger - for Godot specifically.
    /// Initialized one from DI by calling Log.Init().
    /// </summary>
    public static class Log
    {
        private static volatile ILogger _impl = NullLogger.Instance;

        public static void Init(ILogger logger)
        {
            _impl = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public static void SetLogger(ILogger logger) => Init(logger);

        public static bool IsEnabled(LogLevel level) => _impl.IsEnabled(level);

        public static void LogEvent(
            LogLevel level,
            string messageTemplate,
            IReadOnlyDictionary<string, object?>? props = null,
            Exception? exception = null,
            string? category = null,
            string? scene = null,
            string? nodePath = null,
            [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0)
        => _impl.Log(level, messageTemplate, props, exception, category, scene, nodePath, member, file, line);

        public static void Trace(string messageTemplate,
            IReadOnlyDictionary<string, object?>? props = null, string? category = null,
            string? scene = null, string? nodePath = null,
            [CallerMemberName] string? member = null, [CallerFilePath] string? file = null, [CallerLineNumber] int line = 0)
        => _impl.Trace(messageTemplate, props, category, scene, nodePath, member, file, line);

        public static void Debug(string messageTemplate,
            IReadOnlyDictionary<string, object?>? props = null, string? category = null,
            string? scene = null, string? nodePath = null,
            [CallerMemberName] string? member = null, [CallerFilePath] string? file = null, [CallerLineNumber] int line = 0)
        => _impl.Debug(messageTemplate, props, category, scene, nodePath, member, file, line);

        public static void Info(string messageTemplate,
            IReadOnlyDictionary<string, object?>? props = null, string? category = null,
            string? scene = null, string? nodePath = null,
            [CallerMemberName] string? member = null, [CallerFilePath] string? file = null, [CallerLineNumber] int line = 0)
        => _impl.Info(messageTemplate, props, category, scene, nodePath, member, file, line);

        public static void Warn(string messageTemplate,
            IReadOnlyDictionary<string, object?>? props = null, string? category = null,
            string? scene = null, string? nodePath = null,
            [CallerMemberName] string? member = null, [CallerFilePath] string? file = null, [CallerLineNumber] int line = 0)
        => _impl.Warn(messageTemplate, props, category, scene, nodePath, member, file, line);

        public static void Error(string messageTemplate,
            IReadOnlyDictionary<string, object?>? props = null, string? category = null,
            string? scene = null, string? nodePath = null,
            [CallerMemberName] string? member = null, [CallerFilePath] string? file = null, [CallerLineNumber] int line = 0)
        => _impl.Error(messageTemplate, props, category, scene, nodePath, member, file, line);

        public static void Fatal(string messageTemplate,
            IReadOnlyDictionary<string, object?>? props = null, string? category = null,
            string? scene = null, string? nodePath = null,
            [CallerMemberName] string? member = null, [CallerFilePath] string? file = null, [CallerLineNumber] int line = 0)
        => _impl.Fatal(messageTemplate, props, category, scene, nodePath, member, file, line);

        public static void Flush()
        {
            _impl.Flush();
        }
    }
}

