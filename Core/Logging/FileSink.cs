using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Godot;

namespace Neuma.Core.Logging
{
    public sealed class FileSink : ILogSink
    {
        private readonly object _lock = new();
        private readonly string _dir;
        private string _currentFile;
        private readonly long _maxBytes;
        private readonly int _maxFiles;

        public LogLevel MinLevel { get; }

        public FileSink(
            LogLevel minLevel = LogLevel.Info,
            long maxBytes = 5 * 1024 * 1024,
            int maxFiles = 10,
            string? directory = null,
            string filePrefix = "game")
        {
            MinLevel = minLevel;
            _maxBytes = maxBytes;
            _maxFiles = Math.Max(1, maxFiles);

            if (!string.IsNullOrEmpty(directory))
            {
                if (directory.StartsWith("user://"))
                {
                    _dir = Godot.ProjectSettings.GlobalizePath(directory);
                }
                else
                {
                    _dir = System.IO.Path.IsPathRooted(directory) ? directory : System.IO.Path.GetFullPath(directory);
                }
            }
            else
            {
                _dir = Godot.ProjectSettings.GlobalizePath("user://logs");
            }

            try
            {
                Directory.CreateDirectory(_dir);
            }
            catch
            {

            }

            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            _currentFile = Path.Combine(_dir, $"{filePrefix}-{date}.log");
        }

        public void Emit(LogEvent e)
        {
            try
            {
                var line = FormatLine(e);

                lock (_lock)
                {
                    RotateIfNeeded();
                    File.AppendAllText(_currentFile, line + System.Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {

            }
        }

        private void RotateIfNeeded()
        {
            try
            {
                var fi = new FileInfo(_currentFile);
                if (fi.Exists && fi.Length > _maxBytes)
                {
                    var ts = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                    var name = Path.GetFileNameWithoutExtension(_currentFile);
                    var ext = Path.GetExtension(_currentFile);
                    var rotated = Path.Combine(_dir, $"{name}.{ts}{ext}");

                    File.Move(_currentFile, rotated, overwrite: false);

                    CleanupOldFiles();
                }
            }
            catch
            {

            }
        }

        private void CleanupOldFiles()
        {
            try
            {
                var pattern = Path.GetFileNameWithoutExtension(_currentFile);
                var ext = Path.GetExtension(_currentFile);

                var files = Directory.GetFiles(_dir, $"{pattern}*{ext}")
                    .OrderByDescending(f => new FileInfo(f).LastWriteTimeUtc)
                    .ToList();

                var keep = _maxFiles;
                for (int i = keep; i < files.Count; i++)
                {
                    try
                    {
                        File.Delete(files[i]);
                    }
                    catch
                    {

                    }
                }
            }
            catch
            {

            }
        }

        private static string FormatLine(LogEvent e)
        {
            var sb = new StringBuilder(256);
            sb.Append('[').Append(e.UtcTime.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append(" UTC] ");
            sb.Append('[').Append(e.Level).Append(']');
            if (!string.IsNullOrEmpty(e.Category))
            {
                sb.Append('[').Append(e.Category).Append(']');
            }
            sb.Append(' ').Append(RenderMessage(e.MessageTemplate, e.Props));

            if (e.Exception != null)
            {
                sb.Append(" | EXC ").Append(e.Exception.GetType().Name)
                    .Append(": ").Append(e.Exception.Message)
                    .Append(" | ").Append(e.Exception.StackTrace);
            }

            if (!string.IsNullOrEmpty(e.Member))
            {
                sb.Append(" (").Append(e.Member).Append('@').Append(TrimPath(e.File)).Append(':').Append(e.Line).Append(')');
            }

            if (!string.IsNullOrEmpty(e.Scene) || !string.IsNullOrEmpty(e.NodePath))
            {
                sb.Append(" [").Append(e.Scene).Append('|').Append(e.NodePath).Append(']');
            }

            return sb.ToString();
        }

        private static string RenderMessage(string template, IReadOnlyDictionary<string, object?>? props)
        {
            if (props is null || props.Count == 0)
            {
                return template ?? string.Empty;
            }

            var result = template ?? string.Empty;

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

