using System;
using System.Collections.Generic;

namespace Neuma.Core.TextFeed
{
    public interface ITextFeedService
    {
        event EventHandler<TextFeedMessageEventArgs>? OnMessageEnqueued;
        void Enqueue(TextFeedMessage message);
        void EnqueueRange(IEnumerable<TextFeedMessage> messages);
        TextFeedMessage? TryDequeueNext();
        TextFeedMessage? PeekNext();
        void Clear();
        void Clear(Func<TextFeedMessage, bool> predicate);
        int PendingCount { get; }
        IReadOnlyCollection<TextFeedMessage> GetPending();
    }
}

