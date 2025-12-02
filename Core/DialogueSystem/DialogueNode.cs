using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Neuma.Core.DialogueSystem
{
    public abstract class DialogueNode
    {
        public string Id { get; }
        public DialogueNodeType Type { get; }
        public IReadOnlyList<string>? Tags { get; }
        public IReadOnlyDictionary<string, string>? Metadata { get; }
        public object? OptionalContent { get; }

        protected DialogueNode(string id, DialogueNodeType type, IEnumerable<string>? tags = null,
            IDictionary<string, string>? metadata = null, object? optionalContent = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Node id cannot be null or whitespace.", nameof(id));
            }

            Id = id;
            Type = type;
            OptionalContent = optionalContent;

            if (tags != null)
            {
                var filtered = new List<string>();
                foreach (var t in tags)
                {
                    if (!string.IsNullOrWhiteSpace(t))
                    {
                        filtered.Add(t);
                    }
                }

                Tags = new ReadOnlyCollection<string>(filtered);
            }

            if (metadata != null)
            {
                var dict = new Dictionary<string, string>(metadata.Count);
                foreach (var kvp in metadata)
                {
                    if (!string.IsNullOrWhiteSpace(kvp.Key))
                    {
                        dict[kvp.Key] = kvp.Value ?? string.Empty;
                    }
                }
                Metadata = new ReadOnlyDictionary<string, string>(dict);
            }
        }
    }
}

