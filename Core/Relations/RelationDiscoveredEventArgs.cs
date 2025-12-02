using System;

namespace Neuma.Core.Relations
{
    public sealed class RelationDiscoveredEventArgs : EventArgs
    {
        public RelationDefinition Relation { get; }

        public RelationDiscoveredEventArgs(RelationDefinition relation)
        {
            Relation = relation ?? throw new ArgumentNullException(nameof(relation));
        }
    }
}

