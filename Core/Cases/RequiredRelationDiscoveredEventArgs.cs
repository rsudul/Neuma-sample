using System;

namespace Neuma.Core.Cases
{
    public class RequiredRelationDiscoveredEventArgs : EventArgs
    {
        public string RelationId { get; }

        public RequiredRelationDiscoveredEventArgs(string relationId)
        {
            RelationId = relationId ?? throw new ArgumentNullException(nameof(relationId));
        }
    }
}

