using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Neuma.Core.Cases;
using Neuma.Core.EvidenceSystem;
using Neuma.Core.Relations;
using Neuma.Core.TerminalFeed;

namespace Neuma.Core.TerminalApps.EvidenceApp
{
    public sealed class EvidenceTerminalApp : ITerminalApp, IAnchorSelectable
    {
        private readonly IEvidenceRepository _repository;
        private readonly ICaseController _caseController;
        private readonly IEvidenceUnlockService _unlockService;

        private List<Evidence> _currentFilteredList = new();
        private int _selectedIndex = 0;

        private const int VisibleItemsCount = 10;
        private int _scrollOffset = 0;

        public string AppId => "com.neumavoid.evidence";

        public event EventHandler<TerminalAppStateChangedEventArgs> OnStateChanged;
        public event EventHandler<AnchorSelectedEventArgs>? OnAnchorSelected;

        public EvidenceTerminalApp(IEvidenceRepository repository,
            ICaseController caseController,
            IEvidenceUnlockService unlockService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _caseController = caseController ?? throw new ArgumentNullException(nameof(caseController));
            _unlockService = unlockService ?? throw new ArgumentNullException(nameof(unlockService));
        }

        public void OnOpen()
        {
            var oldState = GetCurrentContext();

            var caseId = _caseController.CurrentCaseId;
            if (string.IsNullOrEmpty(caseId))
            {
                _currentFilteredList.Clear();
            }
            else
            {
                var allEvidence = _repository.GetAll(caseId);
                _currentFilteredList = allEvidence.Where(e => _unlockService.IsDiscovered(caseId, e.EvidenceId)).ToList();
            }

            _selectedIndex = 0;

            var newState = GetCurrentContext();

            OnStateChanged?.Invoke(this, new TerminalAppStateChangedEventArgs(oldState, newState));
        }

        public void OnClose()
        {
            _currentFilteredList.Clear();
            _selectedIndex = 0;
        }

        public void HandleCommand(TerminalCommand command)
        {
            if (_currentFilteredList.Count == 0)
            {
                return;
            }

            var oldState = GetCurrentContext();
            bool stateChanged = false;

            switch (command)
            {
                case TerminalCommand.NavigateUp:
                    if (_selectedIndex > 0)
                    {
                        _selectedIndex--;
                        stateChanged = true;
                    }
                    break;

                case TerminalCommand.NavigateDown:
                    if (_selectedIndex < _currentFilteredList.Count - 1)
                    {
                        _selectedIndex++;
                        stateChanged = true;
                    }
                    break;

                case TerminalCommand.Confirm:
                    if (_currentFilteredList.Count > 0 && _selectedIndex >= 0)
                    {
                        var selectedEvidence = _currentFilteredList[_selectedIndex];

                        var anchor = AnchorId.ForEvidence(selectedEvidence.CaseId, selectedEvidence.EvidenceId);

                        OnAnchorSelected?.Invoke(this, new AnchorSelectedEventArgs(anchor, selectedEvidence.Title));
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
            if (_currentFilteredList.Count == 0)
            {
                return;
            }

            if (normalizedPosition.Y < 0.0f || normalizedPosition.Y > 1.0f)
            {
                return;
            }

            int relativeIndex = Mathf.FloorToInt(normalizedPosition.Y * VisibleItemsCount);
            relativeIndex = Mathf.Clamp(relativeIndex, 0, VisibleItemsCount - 1);

            int absoluteIndex = _scrollOffset + relativeIndex;

            if (absoluteIndex >= _currentFilteredList.Count)
            {
                return;
            }

            if (_selectedIndex != absoluteIndex || isClick)
            {
                var oldState = GetCurrentContext();

                _selectedIndex = absoluteIndex;

                if (isClick)
                {
                    var selectedEvidence = _currentFilteredList[_selectedIndex];
                    var anchor = AnchorId.ForEvidence(selectedEvidence.CaseId, selectedEvidence.EvidenceId);
                    OnAnchorSelected?.Invoke(this, new AnchorSelectedEventArgs(anchor, selectedEvidence.Title));
                }

                var newState = GetCurrentContext();

                OnStateChanged?.Invoke(this, new TerminalAppStateChangedEventArgs(oldState, newState));
            }
        }

        public ITerminalViewContext GetCurrentContext()
        {
            var caseId = _caseController.CurrentCaseId ?? string.Empty;

            var summaries = _currentFilteredList.Select(e => new EvidenceSummary(
                e.EvidenceId,
                e.Title,
                e.Type,
                _unlockService.IsNew(caseId, e.EvidenceId)
             )).ToList();

            EvidenceDetailViewData? detailView = null;

            if (_currentFilteredList.Count > 0 && _selectedIndex >= 0 && _selectedIndex < _currentFilteredList.Count)
            {
                var selectedDomainObj = _currentFilteredList[_selectedIndex];

                detailView = new EvidenceDetailViewData(
                    selectedDomainObj.EvidenceId,
                    selectedDomainObj.Title,
                    selectedDomainObj.Description,
                    selectedDomainObj.AssetId ?? string.Empty,
                    selectedDomainObj.Metadata ?? new Dictionary<string, string>(),
                    selectedDomainObj.Tags ?? new List<string>()
                 );
            }

            return new EvidenceAppViewContext(caseId, summaries, _selectedIndex, detailView);
        }

        private void LoadData()
        {
            var caseId = _caseController.CurrentCaseId;
            if (string.IsNullOrEmpty(caseId))
            {
                _currentFilteredList.Clear();
                _selectedIndex = 0;
                return;
            }

            var allEvidence = _repository.GetAll(caseId);

            _currentFilteredList = allEvidence.Where(e => _unlockService.IsDiscovered(caseId, e.EvidenceId)).ToList();

            _selectedIndex = 0;
        }
    }
}

