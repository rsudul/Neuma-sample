using System.Collections.Generic;

namespace Neuma.Core.TranscriptSystem
{
    public sealed class TranscriptData
    {
        public string CaseId { get; init; } = string.Empty;
        public string TranscriptId { get; init; } = string.Empty;
        public List<TranscriptLineData>? Lines { get; init; }
    }
}

