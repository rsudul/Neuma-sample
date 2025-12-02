using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Neuma.Core.Cases;
using Neuma.Core.Relations;
using Neuma.Core.TerminalFeed;
using Neuma.Core.TranscriptSystem;

namespace Neuma.Core.TerminalApps.TranscriptApp
{
    public sealed class TranscriptTerminalApp : ITerminalApp, IAnchorSelectable
    {
        private readonly ITranscriptHistoryRepository _historyRepository;
        private readonly ICaseController _caseController;

        private List<IGrouping<string, TranscriptLine>> _groupedSessions = new();
        private int _selectedSessionIndex = 0;
        private int _selectedLineIndex = -1;

        private const int VisibleLinesCount = 16;
        private int _scrollOffset = 0;

        public string AppId => "com.neumavoid.transcript";

        public event EventHandler<TerminalAppStateChangedEventArgs>? OnStateChanged;
        public event EventHandler<AnchorSelectedEventArgs>? OnAnchorSelected;

        public TranscriptTerminalApp(ITranscriptHistoryRepository historyRepository, ICaseController caseController)
        {
            _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
            _caseController = caseController ?? throw new ArgumentNullException(nameof(caseController));
        }

        public void OnOpen()
        {
            var oldState = GetCurrentContext();
            LoadData();
            var newState = GetCurrentContext();

            OnStateChanged?.Invoke(this, new TerminalAppStateChangedEventArgs(oldState, newState));
        }

        public void OnClose()
        {
            _groupedSessions.Clear();
            _selectedSessionIndex = 0;
            _selectedLineIndex = -1;
        }

        public void HandleCommand(TerminalCommand command)
        {
            if (_groupedSessions.Count == 0)
            {
                return;
            }

            var oldState = GetCurrentContext();
            bool stateChanged = false;

            var currentLinesCount = _groupedSessions[_selectedSessionIndex].Count();

            switch (command)
            {
                case TerminalCommand.NavigateUp:
                    if (_selectedLineIndex == -1)
                    {
                        if (_selectedSessionIndex > 0)
                        {
                            _selectedSessionIndex--;
                            stateChanged = true;
                        }
                    }
                    else
                    {
                        if (_selectedLineIndex > 0)
                        {
                            _selectedLineIndex--;
                            stateChanged = true;
                        }
                    }
                    break;

                case TerminalCommand.NavigateDown:
                    if (_selectedLineIndex == -1)
                    {
                        if (_selectedSessionIndex < _groupedSessions.Count - 1)
                        {
                            _selectedSessionIndex++;
                            stateChanged = true;
                        }
                    }
                    else
                    {
                        if (_selectedLineIndex < currentLinesCount - 1)
                        {
                            _selectedLineIndex++;
                            stateChanged = true;
                        }
                    }
                    break;

                case TerminalCommand.NavigateRight:
                case TerminalCommand.Confirm:
                    if (_selectedLineIndex == -1 && currentLinesCount > 0)
                    {
                        if (command == TerminalCommand.Confirm || command == TerminalCommand.NavigateRight)
                        {
                            _selectedLineIndex = 0;
                            stateChanged = true;
                        }
                    }
                    else if (_selectedLineIndex >= 0 && command == TerminalCommand.Confirm)
                    {
                        SelectCurrentLineAsAnchor();
                    }
                    break;

                case TerminalCommand.NavigateLeft:
                case TerminalCommand.Cancel:
                    if (_selectedLineIndex >= 0)
                    {
                        _selectedLineIndex = -1;
                        stateChanged = true;
                    }
                    break;
            }

            if (stateChanged)
            {
                var newState = GetCurrentContext();
                OnStateChanged?.Invoke(this, new TerminalAppStateChangedEventArgs(oldState, newState));
            }
        }

        public void HandlePointer(Vector2 normalizedPosition, bool isClick)
        {
            if (_groupedSessions.Count == 0)
            {
                return;
            }

            if (normalizedPosition.Y < 0.0f || normalizedPosition.Y > 1.0f)
            {
                return;
            }

            int relativeIndex = Mathf.FloorToInt(normalizedPosition.Y * VisibleLinesCount);
            relativeIndex = Mathf.Clamp(relativeIndex, 0, VisibleLinesCount - 1);

            int absoluteIndex = _scrollOffset + relativeIndex;

            var currentSessionLines = _groupedSessions[_selectedSessionIndex];
            int totalLines = currentSessionLines.Count();

            if (absoluteIndex >= totalLines)
            {
                return;
            }

            if (_selectedLineIndex != absoluteIndex || isClick)
            {
                var oldState = GetCurrentContext();

                _selectedLineIndex = absoluteIndex;

                if (isClick)
                {
                    SelectCurrentLineAsAnchor();
                }

                var newState = GetCurrentContext();

                OnStateChanged?.Invoke(this, new TerminalAppStateChangedEventArgs(oldState, newState));
            }
        }

        private void SelectCurrentLineAsAnchor()
        {
            var group = _groupedSessions[_selectedSessionIndex];
            var line = group.OrderBy(l => l.Index).ElementAt(_selectedLineIndex);
            var anchor = AnchorId.ForTranscript(line.CaseId, line.LineId);
            var displayName = $"{line.SpeakerId}: {Truncate(line.Text, 30)}";
            OnAnchorSelected?.Invoke(this, new AnchorSelectedEventArgs(anchor, displayName));
        }

        public ITerminalViewContext GetCurrentContext()
        {
            var caseId = _caseController.CurrentCaseId ?? string.Empty;

            var sessionSummaries = new List<TranscriptSessionSummary>();
            foreach (var group in _groupedSessions)
            {
                sessionSummaries.Add(new TranscriptSessionSummary(group.Key, group.Count()));
            }

            var currentLines = new List<TranscriptLineView>();

            if (_groupedSessions.Count > 0)
            {
                var selectedGroup = _groupedSessions[_selectedSessionIndex];

                foreach (var line in selectedGroup.OrderBy(l => l.Index))
                {
                    currentLines.Add(new TranscriptLineView(line.SpeakerId, line.Text, FormatTime(line.TimeOffsetSeconds)));
                }
            }

            return new TranscriptAppViewContext(caseId, sessionSummaries, _selectedSessionIndex, currentLines,
                _selectedLineIndex == -1 ? null : _selectedLineIndex);
        }

        private void LoadData()
        {
            var caseId = _caseController.CurrentCaseId;
            if (string.IsNullOrEmpty(caseId))
            {
                _groupedSessions.Clear();
                return;
            }

            var allLines = _historyRepository.GetAllLinesForCase(caseId);

            _groupedSessions = allLines.GroupBy(line => line.TranscriptId).ToList();

            _selectedSessionIndex = 0;
        }

        private static string FormatTime(double? seconds)
        {
            if (!seconds.HasValue)
            {
                return "00:00";
            }

            var ts = TimeSpan.FromSeconds(seconds.Value);
            return $"{(int)ts.TotalMinutes:D2}:{ts.Seconds:D2}";
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }
    }
}

