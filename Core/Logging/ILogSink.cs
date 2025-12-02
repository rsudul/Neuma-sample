namespace Neuma.Core.Logging
{
    /// <summary>
    /// Destination for log events (console, file, overlay, remote, etc.).
    /// </summary>
    public interface ILogSink
    {
        LogLevel MinLevel { get; }

        void Emit(LogEvent e);
    }
}

