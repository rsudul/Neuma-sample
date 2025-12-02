using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Neuma.Core.EvidenceSystem
{
    [Serializable]
    public sealed class Evidence
    {
        public string EvidenceId { get; }
        public string CaseId { get; }
        public EvidenceType Type { get; }
        public string Title { get; }
        public string Description { get; }
        public IReadOnlyList<string>? Tags { get; }
        public IReadOnlyDictionary<string, string>? Metadata { get; }
        public string? AssetId { get; }
        public string? SourcePath { get; }
        public object? OptionalContent { get; }

        public Evidence(string evidenceId,
            string caseId,
            EvidenceType type,
            string title,
            string description,
            IEnumerable<string>? tags = null,
            IDictionary<string, string>? metadata = null,
            string? assetId = null,
            string? sourcePath = null,
            object? optionalContent = null)
        {
            if (string.IsNullOrWhiteSpace(evidenceId))
            {
                throw new ArgumentException("EvidenceId cannot be null or whitespace.", nameof(evidenceId));
            }

            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title cannot be null or whitespace.", nameof(title));
            }

            EvidenceId = evidenceId;
            CaseId = caseId;
            Type = type;
            Title = title;
            Description = description ?? string.Empty;
            AssetId = string.IsNullOrWhiteSpace(assetId) ? null : assetId;
            SourcePath = string.IsNullOrWhiteSpace(sourcePath) ? null : sourcePath;
            OptionalContent = optionalContent;

            if (tags != null)
            {
                var tagList = new List<string>();
                foreach (var tag in tags)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        tagList.Add(tag);
                    }
                }

                Tags = new ReadOnlyCollection<string>(tagList);
            }

            if (metadata != null)
            {
                var dict = new Dictionary<string, string>(metadata.Count);
                foreach (var kv in metadata)
                {
                    if (!string.IsNullOrWhiteSpace(kv.Key))
                    {
                        dict[kv.Key] = kv.Value ?? string.Empty;
                    }
                }

                Metadata = new ReadOnlyDictionary<string, string>(dict);
            }
        }

        public override string ToString()
        {
            return $"{EvidenceId} ({CaseId}) [{Type}] - {Title}";
        }
    }
}

