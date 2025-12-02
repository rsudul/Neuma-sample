using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Neuma.Core.DataLoading;

namespace Neuma.Core.DialogueSystem
{
    public sealed class DialogueRepository : IDialogueRepository
    {
        private readonly IDataLoader<DialogueData> _dataLoader;
        private readonly string _dataRoot;

        private readonly Dictionary<string, Dictionary<string, Dialogue>> _cache =
            new Dictionary<string, Dictionary<string, Dialogue>>(StringComparer.OrdinalIgnoreCase);

        private readonly object _syncRoot = new();

        public DialogueRepository(IDataLoader<DialogueData> dataLoader, string dataRoot = "Data")
        {
            _dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));

            if (string.IsNullOrWhiteSpace(dataRoot))
            {
                throw new ArgumentException("Data root cannot be null or whitespace.", nameof(dataRoot));
            }

            _dataRoot = dataRoot;
        }

        public Dialogue Get(string caseId, string dialogueId)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            if (string.IsNullOrEmpty(dialogueId))
            {
                throw new ArgumentException("DialogueId cannot be null or whitespace.", nameof(dialogueId));
            }

            lock (_syncRoot)
            {
                if (_cache.TryGetValue(caseId, out var byDialogue) &&
                    byDialogue.TryGetValue(dialogueId, out var existing))
                {
                    return existing;
                }
            }

            var loaded = LoadDialogue(caseId, dialogueId);

            lock (_syncRoot)
            {
                if (!_cache.TryGetValue(caseId, out var byDialogue))
                {
                    byDialogue = new Dictionary<string, Dialogue>(StringComparer.OrdinalIgnoreCase);
                    _cache[caseId] = byDialogue;
                }

                if (!byDialogue.ContainsKey(dialogueId))
                {
                    byDialogue[dialogueId] = loaded;
                }

                return byDialogue[dialogueId];
            }
        }

        public bool TryGet(string caseId, string dialogueId, out Dialogue? dialogue)
        {
            try
            {
                dialogue = Get(caseId, dialogueId);
                return true;
            }
            catch
            {
                dialogue = null;
                return false;
            }
        }

        private Dialogue LoadDialogue(string caseId, string dialogueId)
        {
            var resourceId = BuildResourceId(caseId, dialogueId);

            var dto = _dataLoader.Load(resourceId);
            if (dto == null)
            {
                throw new InvalidOperationException($"DialogueData loader returned null for resourceId '{resourceId}'.");
            }

            ValidateRoot(dto, caseId, dialogueId);

            var nodes = new Dictionary<string, DialogueNode>(StringComparer.OrdinalIgnoreCase);

            foreach (var nodeDto in dto.Nodes)
            {
                if (nodeDto == null)
                {
                    continue;
                }

                var node = MapNode(caseId, dialogueId, nodeDto);
                nodes[node.Id] = node;
            }

            return new Dialogue(caseId, dialogueId, dto.EntryNodeId, nodes);
        }

        private static void ValidateRoot(DialogueData dto, string caseId, string dialogueId)
        {
            if (!string.Equals(dto.CaseId, caseId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"DialogueData caseId '{dto.CaseId}' does not match requested '{caseId}'.");
            }

            if (!string.Equals(dto.DialogueId, dialogueId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"DialogueData dialogueId '{dto.DialogueId}' does not match requested '{dialogueId}'.");
            }

            if (string.IsNullOrWhiteSpace(dto.EntryNodeId))
            {
                throw new InvalidOperationException("DialogueData.EntryNodeId cannot be null or whitespace.");
            }

            if (dto.Nodes == null || dto.Nodes.Count == 0)
            {
                throw new InvalidOperationException("DialogueData.Nodes cannot be empty.");
            }
        }

        private DialogueNode MapNode(string caseId, string dialogueId, DialogueNodeData dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Type))
            {
                throw new InvalidOperationException($"DialogueNode '{dto.Id}' has empty Type.");
            }

            return dto.Type switch
            {
                "Line" => new LineNode(dto.Id, dto.SpeakerId, dto.Text, dto.TranscriptLineId, dto.NextNodeId,
                                       dto.Tags, dto.Metadata, dto.OptionalContent),
                "Choice" => new ChoiceNode(dto.Id, MapChoiceList(dto.Choices), dto.Tags, dto.Metadata, dto.OptionalContent),
                _ => throw new InvalidOperationException($"Unsupported dialogue node type '{dto.Type}' in node '{dto.Id}'.")
            };
        }

        private static IReadOnlyList<DialogueChoice> MapChoiceList(List<DialogueChoiceData>? dtos)
        {
            var list = new List<DialogueChoice>();
            if (dtos == null)
            {
                return list.AsReadOnly();
            }

            foreach (var item in dtos)
            {
                if (item == null)
                {
                    continue;
                }

                list.Add(new DialogueChoice(item.Id, item.Text, item.NextNodeId));
            }

            return new ReadOnlyCollection<DialogueChoice>(list);
        }

        private string BuildResourceId(string caseId, string dialogueId)
        {
            return Path.Combine(_dataRoot, caseId, "dialogues", $"{dialogueId}.json");
        }
    }
}

