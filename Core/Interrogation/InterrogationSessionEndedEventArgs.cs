using System;

namespace Neuma.Core.Interrogation
{
    public sealed class InterrogationSessionEndedEventArgs : EventArgs
    {
        public string? CaseId { get; }
        public string? DialogueId { get; }
        public bool EndedByFlow { get; }

        public InterrogationSessionEndedEventArgs(string? caseId, string? dialogueId, bool endedByFlow)
        {
            CaseId = caseId ?? throw new ArgumentNullException(nameof(caseId));
            DialogueId = dialogueId ?? throw new ArgumentNullException(nameof(dialogueId));
            EndedByFlow = endedByFlow;
        }
    }
}

