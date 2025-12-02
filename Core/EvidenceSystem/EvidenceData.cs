using System.Collections.Generic;

namespace Neuma.Core.EvidenceSystem
{
    public sealed class EvidenceData
    {
        public string CaseId { get; init; } = string.Empty;
        public List<EvidenceDataItem>? Evidence { get; init; }
    }
}

