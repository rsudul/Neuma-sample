using System;

namespace Neuma.Core.TextFeed
{
    public sealed class TextFeedMessage
    {
        public Guid Id { get; }
        public TextFeedSource Source { get; }
        public TextFeedMode Mode { get; }
        public TextFeedPriority Priority { get; }
        public TextFeedStyleKey StyleKey { get; }

        public string Content { get; }

        public string? SpeakerName { get; }

        public bool IsSkippable { get; }

        public bool AutoAdvance { get; }

        public float? TypingSpeedOverride { get; }

        public DateTime Timestamp { get; }

        public TextFeedMessage(
            TextFeedSource source,
            TextFeedMode mode,
            string content,
            TextFeedStyleKey styleKey = TextFeedStyleKey.Default,
            TextFeedPriority priority = TextFeedPriority.Normal,
            string? speakerName = null,
            bool isSkippable = true,
            bool autoAdvance = false,
            float? typingSpeedOverride = null,
            Guid? id = null,
            DateTime? timestampUtc = null)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Content cannot be null or whitespace", nameof(content));
            }

            Source = source;
            Mode = mode;
            Content = content;
            StyleKey = styleKey;
            Priority = priority;
            SpeakerName = speakerName;
            IsSkippable = isSkippable;
            AutoAdvance = autoAdvance;
            TypingSpeedOverride = typingSpeedOverride;
            Id = id ?? Guid.NewGuid();
            Timestamp = timestampUtc ?? DateTime.UtcNow;
        }

        public static TextFeedMessage CreateSimple(TextFeedSource source, TextFeedMode mode, string content,
            TextFeedStyleKey styleKey = TextFeedStyleKey.Default)
        {
            return new TextFeedMessage(source, mode, content, styleKey);
        }
    }
}

