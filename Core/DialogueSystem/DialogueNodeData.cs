using System.Collections.Generic;

namespace Neuma.Core.DialogueSystem
{
    public sealed class DialogueNodeData
    {
        public string Id { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;

        public List<string>? Tags { get; init; }
        public Dictionary<string, string>? Metadata { get; init; }
        public object? OptionalContent { get; init; }

        public string? SpeakerId { get; init; }
        public string? Text { get; init; }
        public string? TranscriptLineId { get; init; }

        public string? NextNodeId { get; init; }
        public List<DialogueChoiceData>? Choices { get; init; }
    }
}

