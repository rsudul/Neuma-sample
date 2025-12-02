using System;
using Neuma.Core.DialogueSystem;

namespace Neuma.Core.Interrogation
{
    public sealed class InterrogationSessionStartedEventArgs : EventArgs
    {
        public string CaseId { get; }
        public string DialogueId { get; }
        public Dialogue Dialogue { get; }
        public DialogueNode EntryNode { get; }

        public InterrogationSessionStartedEventArgs(string caseId, string dialogueId, Dialogue dialogue, DialogueNode entryNode)
        {
            CaseId = caseId ?? throw new ArgumentNullException(nameof(caseId));
            DialogueId = dialogueId ?? throw new ArgumentNullException(nameof(dialogueId));
            Dialogue = dialogue ?? throw new ArgumentNullException(nameof(dialogue));
            EntryNode = entryNode ?? throw new ArgumentNullException(nameof(entryNode));
        }
    }
}

