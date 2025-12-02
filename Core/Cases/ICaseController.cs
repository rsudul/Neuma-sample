using System;

namespace Neuma.Core.Cases
{
    /// <summary>
    /// High-level orchestrator for a single investigation case.
    /// Coordinates interrogation sessions and live transcript recording.
    /// Does NOT know about UI or JSON.
    /// </summary>
    public interface ICaseController
    {
        string? CurrentCaseId { get; }
        bool HasActiveCase { get; }
        bool IsInterrogationActive { get; }

        void StartCase(string caseId);
        void StartInterrogation(string dialogueId);
        void EndInterrogation();
        bool ContinueInterrogation();
        bool SelectChoice(string choiceId);
        void CompleteCase();

        event EventHandler<CaseStartedEventArgs>? OnCaseStarted;
        event EventHandler<CaseCompletedEventArgs>? OnCaseCompleted;
    }
}

