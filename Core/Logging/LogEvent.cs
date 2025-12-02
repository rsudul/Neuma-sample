using System;
using System.Collections.Generic;

namespace Neuma.Core.Logging
{
    /// <summary>
    /// Immutable log entry containing structured context.
    /// </summary>
    public sealed class LogEvent
    {
        public DateTime UtcTime { get; init; } = DateTime.UtcNow;
        public LogLevel Level { get; init; }

        public string? Category { get; init; }

        public string MessageTemplate { get; init; } = string.Empty;

        public IReadOnlyDictionary<string, object?>? Props { get; init; }

        public Exception? Exception { get; init; }

        public string? Scene { get; init; }
        public string? NodePath { get; init; }

        public string? Member { get; init; }
        public string? File { get; init; }
        public int Line { get; init; }
    }
}

