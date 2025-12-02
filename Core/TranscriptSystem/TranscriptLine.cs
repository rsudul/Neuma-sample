using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Neuma.Core.TranscriptSystem
{
    public sealed class TranscriptLine
    {
        public string CaseId { get; }
        public string TranscriptId { get; }
        public string LineId { get; }
        public int Index { get; }
        public string SpeakerId { get; }
        public string Text { get; }
        public double? TimeOffsetSeconds { get; }
        public IReadOnlyList<string>? Tags { get; }
        public IReadOnlyDictionary<string, string>? Metadata { get; }
        public object? OptionalContent { get; }

        public TranscriptLine(string caseId, string transcriptId, string lineId, int index, string speakerId, string text,
            double? timeOffsetSeconds = null, IEnumerable<string>? tags = null, IDictionary<string, string>? metadata = null,
            object? optionalContent = null)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            if (string.IsNullOrWhiteSpace(transcriptId))
            {
                throw new ArgumentException("TranscriptId cannot be null or whitespace.", nameof(transcriptId));
            }

            if (string.IsNullOrWhiteSpace(lineId))
            {
                throw new ArgumentException("LineId cannot be null or whitespace.", nameof(lineId));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative.");
            }

            if (string.IsNullOrWhiteSpace(speakerId))
            {
                throw new ArgumentException("SpeakerId cannot be null or whitespace.", nameof(speakerId));
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Text cannot be null or whitespace.", nameof(text));
            }

            CaseId = caseId;
            TranscriptId = transcriptId;
            LineId = lineId;
            Index = index;
            SpeakerId = speakerId;
            Text = text;
            TimeOffsetSeconds = timeOffsetSeconds;
            OptionalContent = optionalContent;

            if (tags != null)
            {
                var filtered = new List<string>();
                foreach (var t in tags)
                {
                    if (!string.IsNullOrWhiteSpace(t))
                    {
                        filtered.Add(t);
                    }
                }
                Tags = new ReadOnlyCollection<string>(filtered);
            }

            if (metadata != null)
            {
                var dict = new Dictionary<string, string>(metadata.Count);
                foreach (var kvp in metadata)
                {
                    if (!string.IsNullOrWhiteSpace(kvp.Key))
                    {
                        dict[kvp.Key] = kvp.Value ?? string.Empty;
                    }
                }
                Metadata = new ReadOnlyDictionary<string, string>(dict);
            }
        }

        public override string ToString()
        {
            return $"{TranscriptId}/{LineId}: [{SpeakerId}] {Text}";
        }
    }
}

