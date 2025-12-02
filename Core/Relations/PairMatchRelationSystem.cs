using System;
using System.Collections.Generic;

namespace Neuma.Core.Relations
{
    public sealed class PairMatchRelationSystem : IPairMatchService
    {
        private readonly IRelationDefinitionRepository _repository;

        public PairMatchRelationSystem(IRelationDefinitionRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public PairMatchResult MatchPair(AnchorId first, AnchorId second)
        {
            if (first.Equals(second))
            {
                return PairMatchResult.NoMatch();
            }

            var relations = _repository.FindByAnchors(first, second);

            return PairMatchResult.FromRelations(relations);
        }

        public IReadOnlyList<RelationDefinition> GetRelationsForPair(AnchorId first, AnchorId second)
        {
            if (first.Equals(second))
            {
                return Array.Empty<RelationDefinition>();
            }

            return _repository.FindByAnchors(first, second);
        }
    }
}

