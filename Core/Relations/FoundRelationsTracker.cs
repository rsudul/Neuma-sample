using System;
using System.Collections.Generic;
using Neuma.Core.Cases;

namespace Neuma.Core.Relations
{
    public sealed class FoundRelationsTracker : IFoundRelationsTracker
    {
        private readonly ICaseProgressTracker _caseProgress;

        private readonly HashSet<string> _discoveredRelationIds = new();

        public event EventHandler<RelationDiscoveredEventArgs>? OnRelationDiscovered;

        public FoundRelationsTracker(ICaseProgressTracker caseProgress)
        {
            _caseProgress = caseProgress ?? throw new ArgumentNullException(nameof(caseProgress));
        }

        public bool TryRegisterMatch(PairMatchResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (!result.HasMatch)
            {
                return false;
            }

            bool discoveredSomething = false;

            foreach (var relation in result.Relations)
            {
                if (relation == null)
                {
                    continue;
                }

                if (_discoveredRelationIds.Contains(relation.RelationId))
                {
                    continue;
                }

                _discoveredRelationIds.Add(relation.RelationId);
                discoveredSomething = true;

                OnRelationDiscovered?.Invoke(this, new RelationDiscoveredEventArgs(relation));

                _caseProgress.RegisterDiscoveredRelation(relation.RelationId, relation.RelationType);
            }

            return discoveredSomething;
        }
    }
}

