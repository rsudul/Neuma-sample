using System;
using System.Collections.Generic;

namespace Neuma.Core.DialogueSystem
{
    public sealed class LineNode : DialogueNode
    {
        public string? SpeakerId { get; }
        public string? Text { get; }
        public string? TranscriptLineId { get; }
        public string? NextNodeId { get; }

        public LineNode(string id, string? speakerId, string? text, string? transcriptLineId, string? nextNodeId = null,
            IEnumerable<string>? tags = null, IDictionary<string, string>? metadata = null, object? optionalContent = null)
            : base(id, DialogueNodeType.Line, tags, metadata, optionalContent)
        {
            if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(transcriptLineId))
            {
                throw new ArgumentException("LineNode requires at least one of text or transcriptLineId to be non-empty.");
            }

            if (!string.IsNullOrWhiteSpace(speakerId))
            {
                SpeakerId = speakerId;
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                Text = text;
            }

            if (!string.IsNullOrWhiteSpace(transcriptLineId))
            {
                TranscriptLineId = transcriptLineId;
            }

            if (!string.IsNullOrWhiteSpace(nextNodeId))
            {
                NextNodeId = nextNodeId;
            }
        }
    }
}

