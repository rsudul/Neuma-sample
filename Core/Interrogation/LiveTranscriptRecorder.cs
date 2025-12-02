using System;
using System.Collections.Generic;
using Neuma.Core.DialogueSystem;
using Neuma.Core.TranscriptSystem;

namespace Neuma.Core.Interrogation
{
    /// <summary>
    /// Records all spoken lines (LineNode) during an interrogation session
    /// and stores them in ITranscriptHistoryRepository.
    /// 
    /// CaseController must call StartRecording() before starting a session.
    /// After that, this class reacts automatically to node events
    /// coming from IInterrogationSessionController.
    /// </summary>
    public sealed class LiveTranscriptRecorder : IDisposable
    {
        private readonly ITranscriptHistoryRepository _historyRepository;
        private readonly IInterrogationSessionController _sessionController;

        private bool _isRecording;
        private string? _caseId;
        private string? _transcriptId;
        private int _nextIndex;

        private readonly object _syncRoot = new object();

        public bool IsRecording
        {
            get { lock (_syncRoot) return _isRecording; }
        }

        public LiveTranscriptRecorder(ITranscriptHistoryRepository historyRepository,
            IInterrogationSessionController sessionController)
        {
            _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
            _sessionController = sessionController ?? throw new ArgumentNullException(nameof(sessionController));

            Subscribe();
        }

        private void Subscribe()
        {
            _sessionController.OnNodeChanged += HandleNodeChanged;
            _sessionController.OnSessionEnded += HandleSessionEnded;
        }

        private void Unsubscribe()
        {
            _sessionController.OnNodeChanged -= HandleNodeChanged;
            _sessionController.OnSessionEnded -= HandleSessionEnded;
        }

        public void StartRecording(string caseId, string transcriptId)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            if (string.IsNullOrWhiteSpace(transcriptId))
            {
                throw new ArgumentException("TranscriptId cannot be null or whitespace.", nameof(transcriptId));
            }

            lock (_syncRoot)
            {
                _isRecording = true;
                _caseId = caseId;
                _transcriptId = transcriptId;
                _nextIndex = 0;
            }

            _historyRepository.ClearTranscript(caseId, transcriptId);
        }

        public void StopRecording()
        {
            lock (_syncRoot)
            {
                _isRecording = false;
                _caseId = null;
                _transcriptId = null;
                _nextIndex = 0;
            }
        }

        private void HandleNodeChanged(object? sender, InterrogationNodeChangedEventArgs args)
        {
            if (!IsRecording)
            {
                return;
            }

            if (args.Node is not LineNode lineNode)
            {
                return;
            }

            string? caseId, transcriptId;
            int index;

            lock (_syncRoot)
            {
                if (!_isRecording || _caseId == null || _transcriptId == null)
                {
                    return;
                }

                caseId = _caseId;
                transcriptId = _transcriptId;
                index = _nextIndex++;
            }

            var speakerId = lineNode.SpeakerId;
            var text = lineNode.Text;

            if (string.IsNullOrWhiteSpace(speakerId) || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var lineId = index.ToString();

            var transcriptLine = new TranscriptLine(caseId!, transcriptId!, lineId, index, speakerId, text,
                null, null, null, null);

            _historyRepository.AppendLine(transcriptLine);
        }

        private void HandleSessionEnded(object? sender, InterrogationSessionEndedEventArgs args)
        {
            StopRecording();
        }

        public void Dispose()
        {
            Unsubscribe();
        }
    }
}

