using System;
using System.Collections.Generic;
using System.Text;
using Godot;

namespace Neuma.Core.Logging
{
    public sealed class ConsoleSink : ILogSink
    {
        public LogLevel MinLevel { get; }

        public ConsoleSink(LogLevel minLevel = LogLevel.Debug)
        {
            MinLevel = minLevel;
        }

        public void Emit(LogEvent e)
        {
            var sb = new StringBuilder();
            sb.Append('[').Append(DateTime.UtcNow.ToString("HH:mm:ss.fff")).Append(" UTC] ");
            sb.Append('[').Append(e.Level).Append(']');
            if (!string.IsNullOrEmpty(e.Category))
            {
                sb.Append('[').Append(e.Category).Append(']');
            }
            sb.Append(' ').Append(RenderMessage(e.MessageTemplate, e.Props));

            if (e.Exception != null)
            {
                sb.AppendLine().Append(e.Exception.GetType().Name).Append(": ").Append(e.Exception.Message)
                    .AppendLine().Append(e.Exception.StackTrace);
            }

            if (!string.IsNullOrEmpty(e.Member))
            {
                sb.Append("  (").Append(e.Member).Append('@').Append(TrimPath(e.File)).Append(':').Append(e.Line).Append(')');
            }

            var line = sb.ToString();
            if (e.Level >= LogLevel.Error)
            {
                GD.PrintErr(line);
            }
            else
            {
                GD.Print(line);
            }
        }

        private static string RenderMessage(string template, IReadOnlyDictionary<string, object?>? props)
        {
            if (props is null || props.Count == 0)
            {
                return template;
            }

            var result = template;

            foreach (var kv in props)
            {
                result = result.Replace("{" + kv.Key + "}", kv.Value?.ToString() ?? "null");
            }

            return result;
        }

        private static string? TrimPath(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            var i = path.LastIndexOfAny(new[] { '/', '\\' });

            return i >= 0 ? path[(i + 1)..] : path;
        }
    }
}

