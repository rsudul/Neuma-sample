using System;
using Neuma.Core.DialogueSystem;

namespace Neuma.Core.Interrogation
{
    public sealed class InterrogationNodeChangedEventArgs : EventArgs
    {
        public string CaseId { get; }
        public string DialogueId { get; }
        public string NodeId { get; }
        public DialogueNode Node { get; }

        public InterrogationNodeChangedEventArgs(string caseId, string dialogueId, string nodeId, DialogueNode node)
        {
            CaseId = caseId ?? throw new ArgumentNullException(nameof(caseId));
            DialogueId = dialogueId ?? throw new ArgumentNullException(nameof(dialogueId));
            NodeId = nodeId ?? throw new ArgumentNullException(nameof(nodeId));
            Node = node ?? throw new ArgumentNullException(nameof(node));
        }
    }
}

