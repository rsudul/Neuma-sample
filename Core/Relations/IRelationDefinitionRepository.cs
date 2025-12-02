using System.Collections.Generic;

namespace Neuma.Core.Relations
{
    public interface IRelationDefinitionRepository
    {
        IReadOnlyList<RelationDefinition> GetAll();
        IReadOnlyList<RelationDefinition> FindByAnchor(AnchorId anchor);
        IReadOnlyList<RelationDefinition> FindByAnchors(AnchorId a, AnchorId b);
    }
}

