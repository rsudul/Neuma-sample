using System;
using System.Collections.Generic;
using Neuma.Core.Relations;

namespace Neuma.Core.Cases
{
    public sealed class CaseProgressTracker : ICaseProgressTracker
    {
        private readonly CaseProgressDefinition _definition;

        private readonly HashSet<string> _discoveredRelations = new();
        private readonly HashSet<string> _discoveredRequriedRelations = new();

        private CaseStatus _status;
        private int _discoveredContradictionsCount;

        public string CaseId => _definition.CaseId;
        public CaseStatus Status => _status;
        public int DiscoveredContradictionsCount => _discoveredContradictionsCount;
        public bool AllRequiredRelationsDiscovered =>
            _discoveredRequriedRelations.Count >= _definition.RequiredRelationIds.Count;

        public event EventHandler<CaseStatusChangedEventArgs>? OnStatusChanged;
        public event EventHandler<RequiredRelationDiscoveredEventArgs>? OnRequiredRelationDiscovered;

        public CaseProgressTracker(CaseProgressDefinition definition)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _status = CaseStatus.InProgress;
        }

        public bool RegisterDiscoveredRelation(string relationId, RelationType relationType)
        {
            if (string.IsNullOrWhiteSpace(relationId))
            {
                throw new ArgumentException("RelationId cannot be null or whitespace.", nameof(relationId));
            }

            if (!_discoveredRelations.Add(relationId))
            {
                return false;
            }

            if (relationType == RelationType.Contradiction)
            {
                _discoveredContradictionsCount++;
            }

            if (IsRelationRequired(relationId))
            {
                if (_discoveredRelations.Add(relationId))
                {
                    OnRequiredRelationDiscovered?.Invoke(this, new RequiredRelationDiscoveredEventArgs(relationId));
                }
            }

            UpdateStatusIfNeeded();

            return true;
        }

        private bool IsRelationRequired(string relationId)
        {
            foreach (var requiredId in _definition.RequiredRelationIds)
            {
                if (requiredId == relationId)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateStatusIfNeeded()
        {
            if (_status == CaseStatus.ReadyForSubmission || _status == CaseStatus.Submitted)
            {
                return;
            }

            bool meetsMinimumContradictions = _discoveredContradictionsCount >= _definition.MinimumContradictionsToSubmit;

            if (meetsMinimumContradictions && AllRequiredRelationsDiscovered)
            {
                ChangeStatus(CaseStatus.ReadyForSubmission);
            }
        }

        private void ChangeStatus(CaseStatus newStatus)
        {
            if (newStatus == _status)
            {
                return;
            }

            var old = _status;
            _status = newStatus;

            OnStatusChanged?.Invoke(this, new CaseStatusChangedEventArgs(old, newStatus));
        }
    }
}

