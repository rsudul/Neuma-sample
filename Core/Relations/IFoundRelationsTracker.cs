using System;

namespace Neuma.Core.Relations
{
    public interface IFoundRelationsTracker
    {
        bool TryRegisterMatch(PairMatchResult result);
        event EventHandler<RelationDiscoveredEventArgs>? OnRelationDiscovered;
    }
}

