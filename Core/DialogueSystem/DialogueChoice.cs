using System;

namespace Neuma.Core.DialogueSystem
{
    public sealed class DialogueChoice
    {
        public string Id { get; }
        public string Text { get; }
        public string NextNodeId { get; }
        public string? ConditionId { get; }
        public string? EffectId { get; }

        public DialogueChoice(string id, string text, string nextNodeId, string? conditionId = null, string? effectId = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Choice id cannot be null or whitespace.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Choice text cannot be null or whitespace.", nameof(text));
            }

            if (string.IsNullOrWhiteSpace(nextNodeId))
            {
                throw new ArgumentException("NextNodeId cannot be null or whitespace.", nameof(nextNodeId));
            }

            Id = id;
            Text = text;
            NextNodeId = nextNodeId;
            ConditionId = string.IsNullOrWhiteSpace(conditionId) ? null : conditionId;
            EffectId = string.IsNullOrWhiteSpace(effectId) ? null : effectId;
        }
    }
}

