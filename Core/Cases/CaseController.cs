using System;
using Neuma.Core.Interrogation;

namespace Neuma.Core.Cases
{
    /// <summary>
    /// High-level orchestrator for a single investigation case.
    /// 
    /// Responsibilities:
    /// - Track the currently active case id.
    /// - Start and end interrogation sessions for the active case.
    /// - Start and stop live transcript recording for each interrogation.
    /// 
    /// It does NOT:
    /// - Talk to UI directly.
    /// - Load JSON or access the filesystem.
    /// - Manage case progress or branching logic (handled by separate systems).
    /// </summary>
    public sealed class CaseController : ICaseController
    {
        private readonly ICaseRepository _caseRepository;
        private readonly IInterrogationSessionController _interrogationSessionController;
        private readonly LiveTranscriptRecorder _liveTranscriptRecorder;

        private string? _currentCaseId;
        private CaseDefinition? _currentCaseDefinition;
        private bool _isCaseCompleted;

        private readonly object _syncRoot = new object();

        public string? CurrentCaseId
        {
            get { lock (_syncRoot) return _currentCaseId; }
        }

        public bool HasActiveCase
        {
            get { lock (_syncRoot) return _currentCaseId != null; }
        }

        public bool IsInterrogationActive => _interrogationSessionController.IsActive;

        public event EventHandler<CaseStartedEventArgs>? OnCaseStarted;
        public event EventHandler<CaseCompletedEventArgs>? OnCaseCompleted;

        public CaseController(ICaseRepository caseRepository,
            IInterrogationSessionController interrogationSessionController,
            LiveTranscriptRecorder liveTranscriptRecorder)
        {
            _caseRepository =
                caseRepository ?? throw new ArgumentNullException(nameof(caseRepository));
            _interrogationSessionController =
                interrogationSessionController ?? throw new ArgumentNullException(nameof(interrogationSessionController));
            _liveTranscriptRecorder =
                liveTranscriptRecorder ?? throw new ArgumentNullException(nameof(liveTranscriptRecorder));
        }

        public void StartCase(string caseId)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            if (_interrogationSessionController.IsActive)
            {
                throw new InvalidOperationException("Cannot change active case while an interrogation session is active.");
            }

            var definition = _caseRepository.Get(caseId);

            lock (_syncRoot)
            {
                _currentCaseId = caseId;
                _currentCaseDefinition = definition;
                _isCaseCompleted = false;
            }

            OnCaseStarted?.Invoke(this, new CaseStartedEventArgs(caseId, definition));
        }

        public void CompleteCase()
        {
            string? caseId;

            lock (_syncRoot)
            {
                if (_currentCaseId == null || _isCaseCompleted)
                {
                    return;
                }

                _isCaseCompleted = true;
                caseId = _currentCaseId;
            }

            OnCaseCompleted?.Invoke(this, new CaseCompletedEventArgs(caseId));
        }

        public void StartInterrogation(string dialogueId)
        {
            if (string.IsNullOrWhiteSpace(dialogueId))
            {
                throw new ArgumentException("DialogueId cannot be null or whitespace.", nameof(dialogueId));
            }

            string? caseId;
            CaseDefinition? caseDefinition;
            bool caseCompleted;

            lock (_syncRoot)
            {
                caseId = _currentCaseId;
                caseDefinition = _currentCaseDefinition;
                caseCompleted = _isCaseCompleted;
            }

            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new InvalidOperationException("Cannot start interrogation without an active case.");
            }

            if (caseDefinition == null)
            {
                throw new InvalidOperationException("CaseDefinition is not loaded.");
            }

            if (caseCompleted)
            {
                throw new InvalidOperationException($"Cannot start interrogation on completed case '{caseId}'.");
            }

            if (_interrogationSessionController.IsActive)
            {
                throw new InvalidOperationException("Cannot start a new interrogation while another session is active.");
            }

            if (!caseDefinition.DialogueToTranscript.TryGetValue(dialogueId, out var transcriptId))
            {
                throw new InvalidOperationException(
                    $"Dialogue '{dialogueId}' does not exist in case '{caseId}' or has no transcriptId.");
            }

            _liveTranscriptRecorder.StartRecording(caseId, transcriptId);

            _interrogationSessionController.StartSession(caseId, dialogueId);
        }

        public void EndInterrogation()
        {
            if (!_interrogationSessionController.IsActive)
            {
                return;
            }

            _interrogationSessionController.EndSession();
        }

        public bool ContinueInterrogation()
        {
            if (!_interrogationSessionController.IsActive)
            {
                return false;
            }

            return _interrogationSessionController.Continue();
        }

        public bool SelectChoice(string choiceId)
        {
            if (string.IsNullOrWhiteSpace(choiceId))
            {
                return false;
            }

            if (!_interrogationSessionController.IsActive)
            {
                return false;
            }

            return _interrogationSessionController.SelectChoice(choiceId);
        }
    }
}

