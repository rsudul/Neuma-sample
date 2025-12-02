using System;
using System.Collections.Generic;
using Neuma.Core.Logging;

namespace Neuma.Core.TextFeed
{
    public sealed class TextFeedService : ITextFeedService
    {
        private readonly object _syncRoot = new();

        private readonly Queue<TextFeedMessage> _critical = new();
        private readonly Queue<TextFeedMessage> _high = new();
        private readonly Queue<TextFeedMessage> _normal = new();
        private readonly Queue<TextFeedMessage> _low = new();

        private const string LogCategory = "Core.TextFeed";

        public event EventHandler<TextFeedMessageEventArgs> OnMessageEnqueued;

        public int PendingCount
        {
            get
            {
                lock (_syncRoot)
                {
                    return _critical.Count + _high.Count + _normal.Count + _low.Count;
                }
            }
        }

        public void Enqueue(TextFeedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            lock (_syncRoot)
            {
                GetQueueForPriority(message.Priority).Enqueue(message);
            }

            Log.Debug($"Enqueued TextFeedMessage {{Id={message.Id}, Source={message.Source}, Mode={message.Mode}, Priority={message.Priority}}}",
                null, LogCategory);

            OnMessageEnqueued?.Invoke(this, new TextFeedMessageEventArgs(message));
        }

        public void EnqueueRange(IEnumerable<TextFeedMessage> messages)
        {
            if (messages == null)
            {
                throw new ArgumentNullException(nameof(messages));
            }

            List<TextFeedMessage>? snapshot = null;
            var nullCount = 0;

            lock (_syncRoot)
            {
                foreach (var message in messages)
                {
                    if (message == null)
                    {
                        nullCount++;
                        continue;
                    }

                    GetQueueForPriority(message.Priority).Enqueue(message);
                    snapshot ??= new List<TextFeedMessage>();
                    snapshot.Add(message);
                }
            }

            if (nullCount > 0)
            {
                Log.Warn($"EnqueueRange encountered {nullCount} null TextFeedMessage entries which were ignored.",
                    null, LogCategory);
            }

            if (snapshot == null || snapshot.Count == 0)
            {
                return;
            }

            Log.Debug($"EnqueueRange enqueued {snapshot.Count} TextFeedMessage entries.", null, LogCategory);

            foreach (var msg in snapshot)
            {
                OnMessageEnqueued?.Invoke(this, new TextFeedMessageEventArgs(msg));
            }
        }

        public TextFeedMessage? TryDequeueNext()
        {
            lock (_syncRoot)
            {
                if (_critical.Count > 0)
                {
                    return _critical.Dequeue();
                }

                if (_high.Count > 0)
                {
                    return _high.Dequeue();
                }

                if (_normal.Count > 0)
                {
                    return _normal.Dequeue();
                }

                if (_low.Count > 0)
                {
                    return _low.Dequeue();
                }

                return null;
            }
        }

        public TextFeedMessage? PeekNext()
        {
            lock (_syncRoot)
            {
                if (_critical.Count > 0)
                {
                    return _critical.Peek();
                }

                if (_high.Count > 0)
                {
                    return _high.Peek();
                }

                if (_normal.Count > 0)
                {
                    return _normal.Peek();
                }

                if (_low.Count > 0)
                {
                    return _low.Peek();
                }

                return null;
            }
        }

        public void Clear()
        {
            lock (_syncRoot)
            {
                _critical.Clear();
                _high.Clear();
                _normal.Clear();
                _low.Clear();
            }

            Log.Debug("TextFeedService.Clear(): all pending messages removed.", null, LogCategory);
        }

        public void Clear(Func<TextFeedMessage, bool> predicate)
        {
            if (predicate == null)
            {
                Log.Warn("TextFeedService.Clear(predicate) called with null predicate. No messages were removed.",
                    null, LogCategory);
                return;
            }

            lock (_syncRoot)
            {
                RebuildQueue(_critical, predicate);
                RebuildQueue(_high, predicate);
                RebuildQueue(_normal, predicate);
                RebuildQueue(_low, predicate);
            }

            Log.Debug("TextFeedService.Clear(predicate): pending messages filtered.", null, LogCategory);
        }

        public IReadOnlyCollection<TextFeedMessage> GetPending()
        {
            lock (_syncRoot)
            {
                if (PendingCount == 0)
                {
                    return Array.Empty<TextFeedMessage>();
                }

                var list = new List<TextFeedMessage>(_critical.Count + _high.Count + _normal.Count + _low.Count);

                list.AddRange(_critical);
                list.AddRange(_high);
                list.AddRange(_normal);
                list.AddRange(_low);

                return list;
            }
        }

        private Queue<TextFeedMessage> GetQueueForPriority(TextFeedPriority priority)
        {
            return priority switch
            {
                TextFeedPriority.Critical => _critical,
                TextFeedPriority.High => _high,
                TextFeedPriority.Low => _low,
                _ => _normal
            };
        }

        private static void RebuildQueue(Queue<TextFeedMessage> queue, Func<TextFeedMessage, bool> predicate)
        {
            if (queue.Count == 0)
            {
                return;
            }

            var tmp = new Queue<TextFeedMessage>(queue.Count);

            while (queue.Count > 0)
            {
                var msg = queue.Dequeue();
                if (!predicate(msg))
                {
                    tmp.Enqueue(msg);
                }
            }

            while (tmp.Count > 0)
            {
                queue.Enqueue(tmp.Dequeue());
            }
        }
    }
}

