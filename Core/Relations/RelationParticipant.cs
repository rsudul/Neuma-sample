namespace Neuma.Core.Relations
{
    [System.Serializable]
    public sealed class RelationParticipant
    {
        public AnchorId Anchor { get; }
        public string? Role { get; }

        public RelationParticipant(AnchorId anchor, string? role = null)
        {
            Anchor = anchor;
            Role = string.IsNullOrWhiteSpace(role) ? null : role;
        }
    }
}

