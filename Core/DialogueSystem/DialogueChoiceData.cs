namespace Neuma.Core.DialogueSystem
{
    public sealed class DialogueChoiceData
    {
        public string Id { get; init; } = string.Empty;
        public string Text { get; init; } = string.Empty;
        public string NextNodeId { get; init; } = string.Empty;
    }
}

