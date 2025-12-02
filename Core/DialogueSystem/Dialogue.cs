using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Neuma.Core.DialogueSystem
{
    public sealed class Dialogue
    {
        public string CaseId { get; }
        public string DialogueId { get; }
        public string EntryNodeId { get; }
        public IReadOnlyDictionary<string, DialogueNode> Nodes { get; }

        public Dialogue(string caseId, string dialogueId, string entryNodeId, IDictionary<string, DialogueNode> nodes)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            if (string.IsNullOrWhiteSpace(dialogueId))
            {
                throw new ArgumentException("DialogueId cannot be null or whitespace.", nameof(dialogueId));
            }

            if (string.IsNullOrWhiteSpace(entryNodeId))
            {
                throw new ArgumentException("EntryNodeId cannot be null or whitespace.", nameof(entryNodeId));
            }

            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (!nodes.ContainsKey(entryNodeId))
            {
                throw new ArgumentException("EntryNodeId must reference an existing node.", nameof(entryNodeId));
            }

            CaseId = caseId;
            DialogueId = dialogueId;
            EntryNodeId = entryNodeId;

            var dict = new Dictionary<string, DialogueNode>(nodes.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in nodes)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    throw new ArgumentException("Node key cannot be null or whitespace.", nameof(nodes));
                }

                if (kvp.Value == null)
                {
                    throw new ArgumentException($"Node '{kvp.Key}' cannot be null.", nameof(nodes));
                }

                dict[kvp.Key] = kvp.Value;
            }

            Nodes = new ReadOnlyDictionary<string, DialogueNode>(dict);
        }

        public bool TryGetNode(string nodeId, out DialogueNode? node)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                node = null;
                return false;
            }

            return Nodes.TryGetValue(nodeId, out node);
        }

        public DialogueNode GetNode(string nodeId)
        {
            if (!TryGetNode(nodeId, out var node) || node == null)
            {
                throw new KeyNotFoundException($"Dialogue node with id '{nodeId}' was not found in dialogue '{DialogueId}'.");
            }

            return node;
        }
    }
}

