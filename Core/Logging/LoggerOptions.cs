using System.Collections.Generic;

namespace Neuma.Core.Logging
{
    public sealed class LoggerOptions
    {
        public LogLevel GlobalMinLevel { get; set; } = LogLevel.Info;
        public Dictionary<string, LogLevel> CategoryOverrides { get; } = new();

        public LogLevel ConsoleMinLevel { get; set; } = LogLevel.Debug;
        public LogLevel FileMinLevel { get; set; } = LogLevel.Info;

        public string FileDirectory { get; set; } = "user://logs";
        public string FilePrefix { get; set; } = "game";
        public long FileMaxBytes { get; set; } = 5 * 1024 * 1024;
        public int FileMaxFiles { get; set; } = 10;

        public bool UseAsync { get; set; } = true;
    }
}

