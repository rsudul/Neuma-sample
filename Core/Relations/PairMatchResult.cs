using System;
using System.Collections.Generic;

namespace Neuma.Core.Relations
{
    [Serializable]
    public sealed class PairMatchResult
    {
        public PairMatchStatus Status { get; }
        public IReadOnlyList<RelationDefinition> Relations { get; }

        public bool HasMatch => Status == PairMatchStatus.MatchFound;

        public PairMatchResult(PairMatchStatus status, IReadOnlyList<RelationDefinition> relations)
        {
            Status = status;
            Relations = relations ?? throw new ArgumentNullException(nameof(relations));
        }

        public static PairMatchResult NoMatch()
        {
            return new PairMatchResult(PairMatchStatus.NoMatch, Array.Empty<RelationDefinition>());
        }

        public static PairMatchResult FromRelations(IReadOnlyList<RelationDefinition> relations)
        {
            if (relations == null)
            {
                throw new ArgumentNullException(nameof(relations));
            }

            if (relations.Count == 0)
            {
                return NoMatch();
            }

            return new PairMatchResult(PairMatchStatus.MatchFound, relations);
        }

        public override string ToString()
        {
            return $"{Status} (relations: {Relations.Count})";
        }
    }
}

