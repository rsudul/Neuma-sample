using System;
using Neuma.Core.DialogueSystem;

namespace Neuma.Core.Interrogation
{
    public interface IInterrogationSessionController
    {
        bool IsActive { get; }
        string? CurrentCaseId { get; }
        string? CurrentDialogueId { get; }
        DialogueNode? CurrentNode { get; }

        void StartSession(string caseId, string dialogueId);
        void EndSession();
        bool Continue();
        bool SelectChoice(string choiceId);

        event EventHandler<InterrogationSessionStartedEventArgs>? OnSessionStarted;
        event EventHandler<InterrogationSessionEndedEventArgs>? OnSessionEnded;
        event EventHandler<InterrogationNodeChangedEventArgs>? OnNodeChanged;
        event EventHandler<InterrogationChoiceSelectedEventArgs>? OnChoiceSelected;
    }
}

