using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Neuma.Core.DataLoading;
using Neuma.Core.EvidenceSystem;

namespace Neuma.Infrastructure.EvidenceSystem
{
    public sealed class EvidenceRepository : IEvidenceRepository
    {
        private readonly IDataLoader<EvidenceData> _dataLoader;
        private readonly string _dataRoot;

        private readonly Dictionary<string, IReadOnlyList<Evidence>> _evidenceCache =
            new Dictionary<string, IReadOnlyList<Evidence>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, Evidence>> _indexCache =
            new Dictionary<string, Dictionary<string, Evidence>>(StringComparer.OrdinalIgnoreCase);

        private readonly object _syncRoot = new object();

        public EvidenceRepository(IDataLoader<EvidenceData> dataLoader, string dataRoot = "Data")
        {
            _dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));

            if (string.IsNullOrWhiteSpace(dataRoot))
            {
                throw new ArgumentException("Data root cannot be null or whitespace.", nameof(dataRoot));
            }

            _dataRoot = dataRoot;
        }

        public IReadOnlyList<Evidence> GetAll(string caseId)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            lock (_syncRoot)
            {
                if (_evidenceCache.TryGetValue(caseId, out var cached))
                {
                    return cached;
                }
            }

            var loaded = LoadCaseEvidence(caseId);

            lock (_syncRoot)
            {
                if (_evidenceCache.TryGetValue(caseId, out var existing))
                {
                    return existing;
                }

                _evidenceCache[caseId] = loaded.List;
                _indexCache[caseId] = loaded.Index;

                return loaded.List;
            }
        }

        public bool TryGet(string caseId, string evidenceId, out Evidence? evidence)
        {
            evidence = null;

            if (string.IsNullOrWhiteSpace(caseId) || string.IsNullOrWhiteSpace(evidenceId))
            {
                return false;
            }

            _ = GetAll(caseId);

            lock (_syncRoot)
            {
                if (_indexCache.TryGetValue(caseId, out var index) &&
                    index.TryGetValue(evidenceId, out var found))
                {
                    evidence = found;
                    return true;
                }
            }

            return false;
        }

        public Evidence Get(string caseId, string evidenceId)
        {
            if (!TryGet(caseId, evidenceId, out var evidence) || evidence == null)
            {
                throw new KeyNotFoundException($"Evidence with id '{evidenceId}' for case '{caseId}' was not found.");
            }

            return evidence;
        }

        private (IReadOnlyList<Evidence> List, Dictionary<string, Evidence> Index) LoadCaseEvidence(string caseId)
        {
            var resourceId = BuildResourceId(caseId);

            var data = _dataLoader.Load(resourceId);
            if (data == null)
            {
                throw new InvalidOperationException(
                    $"Evidence data loader returned null for case '{caseId}' (resource '{resourceId}'.");
            }

            if (!string.IsNullOrWhiteSpace(data.CaseId) &&
                !string.Equals(data.CaseId, caseId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Evidence data case id '{data.CaseId}' does not match requested case id '{caseId}'.");
            }

            var list = new List<Evidence>();
            var index = new Dictionary<string, Evidence>(StringComparer.OrdinalIgnoreCase);

            if (data.Evidence != null)
            {
                foreach (var item in data.Evidence)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.Id))
                    {
                        continue;
                    }

                    var evidence = MapToDomain(caseId, item);
                    list.Add(evidence);
                    index[evidence.EvidenceId] = evidence;
                }
            }

            return (new ReadOnlyCollection<Evidence>(list), index);
        }

        private string BuildResourceId(string caseId)
        {
            return Path.Combine(_dataRoot, caseId, "evidence.json");
        }

        private static Evidence MapToDomain(string caseId, EvidenceDataItem item)
        {
            var type = ParseEvidenceType(item.Type);

            return new Evidence(item.Id, caseId, type, item.Title, item.Description ?? string.Empty, item.Tags,
                item.Metadata, item.AssetId, item.SourcePath, item.OptionalContent);
        }

        private static EvidenceType ParseEvidenceType(string rawType)
        {
            if (string.IsNullOrWhiteSpace(rawType))
            {
                throw new InvalidOperationException("Evidence type string cannot be null or whitespace.");
            }

            if (Enum.TryParse<EvidenceType>(rawType, true, out var result))
            {
                return result;
            }

            throw new InvalidOperationException($"Unknown evidence type: '{rawType}'.");
        }
    }
}

