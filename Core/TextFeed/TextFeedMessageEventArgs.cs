using System;

namespace Neuma.Core.TextFeed
{
    public sealed class TextFeedMessageEventArgs : EventArgs
    {
        public TextFeedMessage Message { get; }
        public TextFeedMessageEventArgs(TextFeedMessage message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}

