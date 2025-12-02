using System;
using Neuma.Core.DialogueSystem;

namespace Neuma.Core.Interrogation
{
    public sealed class InterrogationChoiceSelectedEventArgs : EventArgs
    {
        public string CaseId { get; }
        public string DialogueId { get; }
        public string NodeId { get; }
        public string ChoiceId { get; }
        public DialogueChoice Choice { get; }

        public InterrogationChoiceSelectedEventArgs(string caseId, string dialogueId, string nodeId, string choiceId, DialogueChoice choice)
        {
            CaseId = caseId ?? throw new ArgumentNullException(nameof(caseId));
            DialogueId = dialogueId ?? throw new ArgumentNullException(nameof(dialogueId));
            NodeId = nodeId ?? throw new ArgumentNullException(nameof(nodeId));
            ChoiceId = choiceId ?? throw new ArgumentNullException(nameof(choiceId));
            Choice = choice ?? throw new ArgumentNullException(nameof(choice));
        }
    }
}

