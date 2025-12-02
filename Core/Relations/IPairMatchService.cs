using System.Collections.Generic;

namespace Neuma.Core.Relations
{
    public interface IPairMatchService
    {
        PairMatchResult MatchPair(AnchorId first, AnchorId second);
        IReadOnlyList<RelationDefinition> GetRelationsForPair(AnchorId first, AnchorId second);
    }
}

