using System;
using Neuma.Core.Relations;

namespace Neuma.Core.Cases
{
    public interface ICaseProgressTracker
    {
        string CaseId { get; }
        CaseStatus Status { get; }
        int DiscoveredContradictionsCount { get; }
        bool AllRequiredRelationsDiscovered { get; }

        bool RegisterDiscoveredRelation(string relationId, RelationType relationType);

        event EventHandler<CaseStatusChangedEventArgs>? OnStatusChanged;
        event EventHandler<RequiredRelationDiscoveredEventArgs>? OnRequiredRelationDiscovered;
    }
}

