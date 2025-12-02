using System;
using System.Collections.Generic;
using System.IO;
using Neuma.Core.Cases;
using Neuma.Core.DataLoading;

namespace Neuma.Infrastructure.Cases
{
    /// <summary>
    /// Repository providing read-only access to case definitions.
    /// Loads CaseDefinitionData via IDataLoader and maps it to CaseDefinition.
    /// Results are cached in memory per case id.
    /// </summary>
    public sealed class CaseRepository : ICaseRepository
    {
        private readonly IDataLoader<CaseDefinitionData> _dataLoader;
        private readonly string _dataRoot;

        private readonly Dictionary<string, CaseDefinition> _cache =
            new Dictionary<string, CaseDefinition>(StringComparer.OrdinalIgnoreCase);

        private readonly object _syncRoot = new object();

        public CaseRepository(IDataLoader<CaseDefinitionData> dataLoader, string dataRoot = "Data")
        {
            _dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));

            if (string.IsNullOrWhiteSpace(dataRoot))
            {
                throw new ArgumentException("Data root cannot be null or whitespace.", nameof(dataRoot));
            }

            _dataRoot = dataRoot;
        }

        public CaseDefinition Get(string caseId)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            lock (_syncRoot)
            {
                if (_cache.TryGetValue(caseId, out var existing))
                {
                    return existing;
                }
            }

            var loaded = LoadCase(caseId);

            lock (_syncRoot)
            {
                if (_cache.TryGetValue(caseId, out var existing))
                {
                    return existing;
                }

                _cache[caseId] = loaded;
                return loaded;
            }
        }

        public bool TryGet(string caseId, out CaseDefinition? caseDefinition)
        {
            caseDefinition = null;

            if (string.IsNullOrWhiteSpace(caseId))
            {
                return false;
            }

            lock (_syncRoot)
            {
                if (_cache.TryGetValue(caseId, out var existing))
                {
                    caseDefinition = existing;
                    return true;
                }
            }

            var loaded = LoadCase(caseId);

            lock (_syncRoot)
            {
                if (_cache.TryGetValue(caseId, out var existing))
                {
                    caseDefinition = existing;
                    return true;
                }

                _cache[caseId] = loaded;
                caseDefinition = loaded;
                return true;
            }
        }

        private CaseDefinition LoadCase(string caseId)
        {
            var resourceId = BuildResourceId(caseId);

            var data = _dataLoader.Load(resourceId);
            if (data == null)
            {
                throw new InvalidOperationException(
                    $"Case data loader returned null for case '{caseId}' (resource '{resourceId}'.");
            }

            if (!string.IsNullOrWhiteSpace(data.CaseId) &&
                !string.Equals(data.CaseId, caseId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Case data id '{data.CaseId}' does not match requested case id '{caseId}'.");
            }

            var dialogues = data.Dialogues ?? new List<CaseDialogueData>();

            return new CaseDefinition(caseId, data.EntryDialogueId, dialogues);
        }

        private string BuildResourceId(string caseId)
        {
            return Path.Combine(_dataRoot, caseId, "case.json");
        }
    }
}

