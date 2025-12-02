using System.Collections.Generic;

namespace Neuma.Core.Cases
{
    /// <summary>
    /// Raw case definition loaded from JSON.
    /// This is a data-transfer object (DTO), not validated domain.
    /// </summary>
    public sealed class CaseDefinitionData
    {
        public string CaseId { get; init; } = string.Empty;
        public string EntryDialogueId { get; init; } = string.Empty;
        public List<CaseDialogueData>? Dialogues { get; init; }
    }
}

