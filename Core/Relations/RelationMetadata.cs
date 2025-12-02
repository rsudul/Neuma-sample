using System.Collections.Generic;

namespace Neuma.Core.Relations
{
    [System.Serializable]
    public sealed class RelationMetadata
    {
        public string? Title { get; init; }
        public string? Description { get; init; }
        public int? Severity { get; init; }
        public IReadOnlyList<string>? Tags { get; init; }
    }
}

