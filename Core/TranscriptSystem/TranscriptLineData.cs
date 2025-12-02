using System.Collections.Generic;

namespace Neuma.Core.TranscriptSystem
{
    public sealed class TranscriptLineData
    {
        public string Id { get; init; } = string.Empty;
        public int Index { get; init; }
        public string SpeakerId { get; init; } = string.Empty;
        public string Text { get; init; } = string.Empty;
        public double? TimeOffsetSeconds { get; init; }
        public List<string>? Tags { get; init; }
        public Dictionary<string, string>? Metadata { get; init; }
        public object? OptionalContent { get; init; }
    }
}

