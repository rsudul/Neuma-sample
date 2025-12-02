using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Neuma.Core.DialogueSystem
{
    public sealed class ChoiceNode : DialogueNode
    {
        public IReadOnlyList<DialogueChoice> Choices { get; }

        public ChoiceNode(string id, IEnumerable<DialogueChoice> choices, IEnumerable<string>? tags = null,
            IDictionary<string, string>? metadata = null, object? optionalContent = null)
            : base(id, DialogueNodeType.Choice, tags, metadata, optionalContent)
        {
            if (choices == null)
            {
                throw new ArgumentNullException(nameof(choices));
            }

            var list = new List<DialogueChoice>();
            foreach (var choice in choices)
            {
                if (choice != null)
                {
                    list.Add(choice);
                }
            }

            if (list.Count == 0)
            {
                throw new ArgumentException("ChoiceNode must contain at least one choice.", nameof(choices));
            }

            Choices = new ReadOnlyCollection<DialogueChoice>(list);
        }
    }
}

