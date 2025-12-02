using System;
using System.Collections.Generic;

namespace Neuma.Core.EvidenceSystem
{
    public sealed class EvidenceUnlockService : IEvidenceUnlockService
    {
        private readonly Dictionary<string, HashSet<string>> _unlockedEvidence = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, HashSet<string>> _newEvidence = new(StringComparer.OrdinalIgnoreCase);

        private readonly object _syncRoot = new();

        public event EventHandler<EvidenceUnlockedEventArgs>? OnEvidenceUnlocked;

        public bool IsDiscovered(string caseId, string evidenceId)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            if (string.IsNullOrWhiteSpace(evidenceId))
            {
                throw new ArgumentException("EvidenceId cannot be null or whitespace.", nameof(evidenceId));
            }

            lock (_syncRoot)
            {
                return _unlockedEvidence.TryGetValue(caseId, out var set) && set.Contains(evidenceId);
            }
        }

        public bool IsNew(string caseId, string evidenceId)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            if (string.IsNullOrWhiteSpace(evidenceId))
            {
                throw new ArgumentException("EvidenceId cannot be null or whitespace.", nameof(evidenceId));
            }

            lock (_syncRoot)
            {
                return _newEvidence.TryGetValue(caseId, out var set) && set.Contains(evidenceId);
            }
        }

        public void UnlockEvidence(string caseId, string evidenceId)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            if (string.IsNullOrWhiteSpace(evidenceId))
            {
                throw new ArgumentException("EvidenceId cannot be null or whitespace.", nameof(evidenceId));
            }

            bool contentAdded = false;

            lock (_syncRoot)
            {
                if (!_unlockedEvidence.TryGetValue(caseId, out var unlockedSet))
                {
                    unlockedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    _unlockedEvidence[caseId] = unlockedSet;
                }

                if (unlockedSet.Add(evidenceId))
                {
                    contentAdded = true;

                    if (!_newEvidence.TryGetValue(caseId, out var newSet))
                    {
                        newSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        _newEvidence[caseId] = newSet;
                    }
                    newSet.Add(evidenceId);
                }
            }

            if (contentAdded)
            {
                OnEvidenceUnlocked?.Invoke(this, new EvidenceUnlockedEventArgs(caseId, evidenceId));
            }
        }

        public void MarkAsRead(string caseId, string evidenceId)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            if (string.IsNullOrWhiteSpace(evidenceId))
            {
                throw new ArgumentException("EvidenceId cannot be null or whitespace.", nameof(evidenceId));
            }

            lock (_syncRoot)
            {
                if (_newEvidence.TryGetValue(caseId, out var newSet))
                {
                    if (newSet.Remove(evidenceId))
                    {
                        // event call
                    }
                }
            }
        }

        public Dictionary<string, List<string>> GetSnapshot()
        {
            lock (_syncRoot)
            {
                var snapshot = new Dictionary<string, List<string>>();
                foreach (var kvp in _unlockedEvidence)
                {
                    snapshot[kvp.Key] = new List<string>(kvp.Value);
                }
                return snapshot;
            }
        }
    }
}

