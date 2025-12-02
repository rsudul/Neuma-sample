using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Neuma.Core.Logging
{
    public sealed class Logger : ILogger, IDisposable
    {
        private readonly IReadOnlyList<ILogSink> _sinks;
        private readonly LoggerOptions _options;

        private readonly CancellationTokenSource _cts = new();
        private readonly Channel<LogEvent> _channel;
        private readonly Task _worker;
        private readonly Action<LogEvent>?[] _emit;
        private long _pending;

        private int _fatalInProgress;
        private bool _exceptionHooksAttached;

        public Logger(IEnumerable<ILogSink> sinks, LoggerOptions options)
        {
            _sinks = sinks.ToList();
            _options = options;

            _channel = Channel.CreateUnbounded<LogEvent>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

            var levels = (LogLevel[])Enum.GetValues(typeof(LogLevel));
            _emit = new Action<LogEvent>?[levels.Length];
            for (int i = 0; i < levels.Length; i++)
            {
                var level = levels[i];
                Action<LogEvent>? del = null;
                for (int j = 0; j < _sinks.Count; j++)
                {
                    var sink = _sinks[j];
                    if (sink.MinLevel <= level)
                    {
                        del += sink.Emit;
                    }
                    _emit[i] = del;
                }
            }

            _worker = Task.Run(WorkerLoop);

            AttachExceptionHooks();
        }

        private void AttachExceptionHooks()
        {
            if (_exceptionHooksAttached)
            {
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            _exceptionHooksAttached = true;
        }

        public bool IsEnabled(LogLevel level) => level >= _options.GlobalMinLevel;

        private LogLevel EffectiveMinLevel(string? category)
        {
            if (category is null)
            {
                return _options.GlobalMinLevel;
            }

            var match = _options.CategoryOverrides
                .Where(kv => category.StartsWith(kv.Key, StringComparison.Ordinal))
                .OrderByDescending(kv => kv.Key.Length)
                .FirstOrDefault();

            return match.Equals(default(KeyValuePair<string, LogLevel>))
                ? _options.GlobalMinLevel
                : match.Value;
        }

        public void Log(
            LogLevel level,
            string messageTemplate,
            IReadOnlyDictionary<string, object?>? props = null,
            Exception? exception = null,
            string? category = null,
            string? scene =null,
            string? nodePath = null,
            string? member = null,
            string? file = null,
            int line = 0)
        {
            var min = EffectiveMinLevel(category);
            if (level < min)
            {
                return;
            }

            var e = new LogEvent
            {
                Level = level,
                MessageTemplate = messageTemplate ?? string.Empty,
                Props = props,
                Exception = exception,
                Category = category,
                Scene = scene,
                NodePath = nodePath,
                Member = member,
                File = file,
                Line = line
            };

            Interlocked.Increment(ref _pending);
            _channel.Writer.TryWrite(e);
        }

        public void Trace(string m, IReadOnlyDictionary<string, object?>? p = null, string? c = null, string? s = null, string? n = null, string? member = null, string? file = null, int line = 0)
            => Log(LogLevel.Trace, m, p, null, c, s, n, member, file, line);

        public void Debug(string m, IReadOnlyDictionary<string, object?>? p = null, string? c = null, string? s = null, string? n = null, string? member = null, string? file = null, int line = 0)
            => Log(LogLevel.Debug, m, p, null, c, s, n, member, file, line);

        public void Info(string m, IReadOnlyDictionary<string, object?>? p = null, string? c = null, string? s = null, string? n = null, string? member = null, string? file = null, int line = 0)
            => Log(LogLevel.Info, m, p, null, c, s, n, member, file, line);

        public void Warn(string m, IReadOnlyDictionary<string, object?>? p = null, string? c = null, string? s = null, string? n = null, string? member = null, string? file = null, int line = 0)
            => Log(LogLevel.Warn, m, p, null, c, s, n, member, file, line);

        public void Error(string m, IReadOnlyDictionary<string, object?>? p = null, string? c = null, string? s = null, string? n = null, string? member = null, string? file = null, int line = 0)
            => Log(LogLevel.Error, m, p, null, c, s, n, member, file, line);

        public void Fatal(string m, IReadOnlyDictionary<string, object?>? p = null, string? c = null, string? s = null, string? n = null, string? member = null, string? file = null, int line = 0)
            => Log(LogLevel.Fatal, m, p, null, c, s, n, member, file, line);

        private async Task WorkerLoop()
        {
            var reader = _channel.Reader;
            var token = _cts.Token;

            try
            {
                while (await reader.WaitToReadAsync(token).ConfigureAwait(false))
                {
                    while (reader.TryRead(out var e))
                    {
                        var emitter = _emit[(int)e.Level];
                        try
                        {
                            emitter?.Invoke(e);
                        }
                        catch { }
                        Interlocked.Decrement(ref _pending);
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }

            while (reader.TryRead(out var e))
            {
                try
                {
                    _emit[(int)e.Level]?.Invoke(e);
                }
                catch { }
                finally
                {
                    Interlocked.Decrement(ref _pending);
                }
            }
        }

        public void Flush()
        {
            var sw = new SpinWait();
            while (Interlocked.Read(ref _pending) > 0)
            {
                sw.SpinOnce();
            }
        }

        public void Dispose()
        {
            try
            {
                if (_exceptionHooksAttached)
                {
                    AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
                    TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
                    _exceptionHooksAttached = false;
                }
                
                _channel.Writer.TryComplete();
                _cts.Cancel();
                _worker.Wait(2000);
                Flush();
            }
            catch { }
            finally
            {
                _cts.Dispose();
            }
        }

        private void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex == null)
            {
                return;
            }
            HandleFatalException(ex, "AppDomain.UnhandledException", isTerminating: e.IsTerminating);
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                HandleFatalException(e.Exception, "TaskScheduler.UnobservedTaskException", isTerminating: false);
            }
            finally
            {
                e.SetObserved();
            }
        }

        private void HandleFatalException(Exception ex, string source, bool isTerminating)
        {
            if (Interlocked.Exchange(ref _fatalInProgress, 1) == 1)
            {
                return;
            }

            try
            {
                Log(LogLevel.Fatal,
                    messageTemplate: $"UNHANDLED EXCEPTION ({source}) IsTerminating={{IsTerminating}}",
                    props: new Dictionary<string, object?> { ["IsTerminating"] = isTerminating },
                    exception: ex,
                    category: "Runtime.Fatal");

                Flush();
            }
            catch { }
        }
    }
}

