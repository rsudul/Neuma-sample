using System.Collections.Generic;

namespace Neuma.Core.EvidenceSystem
{
    public sealed class EvidenceDataItem
    {
        public string Id { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public List<string>? Tags { get; init; }
        public Dictionary<string, string>? Metadata { get; init; }
        public string? AssetId { get; init; }
        public string? SourcePath { get; init; }
        public object? OptionalContent { get; init; }
    }
}

