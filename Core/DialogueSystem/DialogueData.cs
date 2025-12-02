using System.Collections.Generic;

namespace Neuma.Core.DialogueSystem
{
    public sealed class DialogueData
    {
        public string CaseId { get; init; } = string.Empty;
        public string DialogueId { get; init; } = string.Empty;
        public string EntryNodeId { get; init; } = string.Empty;
        public List<DialogueNodeData> Nodes { get; init; } = new List<DialogueNodeData>();
    }
}

