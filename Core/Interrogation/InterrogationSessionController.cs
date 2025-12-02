using System;
using Neuma.Core.DialogueSystem;

namespace Neuma.Core.Interrogation
{
    public sealed class InterrogationSessionController : IInterrogationSessionController
    {
        private readonly IDialogueRepository _dialogueRepository;

        private bool _isActive;
        private string? _currentCaseId;
        private string? _currentDialogueId;
        private Dialogue? _currentDialogue;
        private string? _currentNodeId;
        private DialogueNode? _currentNode;

        public bool IsActive => _isActive;
        public string? CurrentCaseId => _currentCaseId;
        public string? CurrentDialogueId => _currentDialogueId;
        public DialogueNode? CurrentNode => _currentNode;

        public event EventHandler<InterrogationSessionStartedEventArgs>? OnSessionStarted;
        public event EventHandler<InterrogationSessionEndedEventArgs>? OnSessionEnded;
        public event EventHandler<InterrogationNodeChangedEventArgs>? OnNodeChanged;
        public event EventHandler<InterrogationChoiceSelectedEventArgs>? OnChoiceSelected;

        public InterrogationSessionController(IDialogueRepository dialogueRepository)
        {
            _dialogueRepository = dialogueRepository ?? throw new ArgumentNullException(nameof(dialogueRepository));
        }

        public void StartSession(string caseId, string dialogueId)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            if (string.IsNullOrWhiteSpace(dialogueId))
            {
                throw new ArgumentException("DialogueId cannot be null or whitespace.", nameof(dialogueId));
            }

            if (_isActive)
            {
                throw new InvalidOperationException("Cannot start a new interrogation session while another one is active.");
            }

            var dialogue = _dialogueRepository.Get(caseId, dialogueId);
            if (!dialogue.TryGetNode(dialogue.EntryNodeId, out var entryNode) || entryNode == null)
            {
                throw new InvalidOperationException(
                    $"Entry node '{dialogue.EntryNodeId}' not found in dialogue '{dialogueId}'.");
            }

            _isActive = true;
            _currentCaseId = caseId;
            _currentDialogueId = dialogueId;
            _currentDialogue = dialogue;

            SetCurrentNodeInternal(dialogue.EntryNodeId, entryNode, false);

            OnSessionStarted?.Invoke(this,
                new InterrogationSessionStartedEventArgs(caseId, dialogueId, dialogue, entryNode));

            RaiseNodeChanged(entryNode);
        }

        public void EndSession()
        {
            if (!_isActive)
            {
                return;
            }

            var caseId = _currentCaseId;
            var dialogueId = _currentDialogueId;

            ClearState();

            OnSessionEnded?.Invoke(this,
                new InterrogationSessionEndedEventArgs(caseId, dialogueId, false));
        }

        public bool Continue()
        {
            if (!_isActive || _currentDialogue == null || _currentNode == null)
            {
                return false;
            }

            if (_currentNode is not LineNode lineNode)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(lineNode.NextNodeId))
            {
                var caseId = _currentCaseId;
                var dialogueId = _currentDialogueId;

                ClearState();

                OnSessionEnded?.Invoke(this,
                    new InterrogationSessionEndedEventArgs(caseId, dialogueId, true));

                return false;
            }

            var nextId = lineNode.NextNodeId!;
            var nextNode = _currentDialogue.GetNode(nextId);

            SetCurrentNodeInternal(nextId, nextNode, true);

            return true;
        }

        public bool SelectChoice(string choiceId)
        {
            if (!_isActive || _currentDialogue == null || _currentNode == null)
            {
                return false;
            }

            if (_currentNode is not ChoiceNode choiceNode)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(choiceId))
            {
                return false;
            }

            var choice = FindChoice(choiceNode, choiceId);
            if (choice == null)
            {
                return false;
            }

            var caseId = _currentCaseId ?? string.Empty;
            var dialogueId = _currentDialogueId ?? string.Empty;
            var nodeId = _currentNodeId ?? choiceNode.Id;

            OnChoiceSelected?.Invoke(this,
                new InterrogationChoiceSelectedEventArgs(caseId, dialogueId, nodeId, choice.Id, choice));

            var nextId = choice.NextNodeId;
            var nextNode = _currentDialogue.GetNode(nextId);

            SetCurrentNodeInternal(nextId, nextNode, true);

            return true;
        }

        private void SetCurrentNodeInternal(string nodeId, DialogueNode node, bool raiseEvent)
        {
            _currentNodeId = nodeId ?? throw new ArgumentNullException(nameof(nodeId));
            _currentNode = node ?? throw new ArgumentNullException(nameof(node));

            if (raiseEvent)
            {
                RaiseNodeChanged(node);
            }
        }

        private void RaiseNodeChanged(DialogueNode node)
        {
            if (!_isActive || _currentCaseId == null || _currentDialogueId == null || _currentNodeId == null)
            {
                return;
            }

            OnNodeChanged?.Invoke(this,
                new InterrogationNodeChangedEventArgs(_currentCaseId, _currentDialogueId, _currentNodeId, node));
        }

        private static DialogueChoice? FindChoice(ChoiceNode choiceNode, string choiceId)
        {
            foreach (var choice in choiceNode.Choices)
            {
                if (string.Equals(choice.Id, choiceId, StringComparison.OrdinalIgnoreCase))
                {
                    return choice;
                }
            }

            return null;
        }

        private void ClearState()
        {
            _isActive = false;
            _currentCaseId = null;
            _currentDialogueId = null;
            _currentDialogue = null;
            _currentNodeId = null;
            _currentNode = null;
        }
    }
}

