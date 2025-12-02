using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Neuma.Core.Relations;

namespace Neuma.Infrastructure.Relations
{
    public sealed class JsonRelationDefinitionRepository : IRelationDefinitionRepository
    {
        private class RelationsJsonRoot
        {
            public List<RelationDefinition> Relations { get; set; } = new();
        }

        private readonly List<RelationDefinition> _relations;
        private readonly Dictionary<AnchorId, List<RelationDefinition>> _relationsByAnchor;

        public JsonRelationDefinitionRepository(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
            {
                throw new FileNotFoundException($"Relations JSON not found: {jsonFilePath}");
            }

            string json = File.ReadAllText(jsonFilePath);

            var root = JsonSerializer.Deserialize<RelationsJsonRoot>(json)
                ?? throw new Exception("Failed to deserialize relations JSON.");

            _relations = new List<RelationDefinition>();

            foreach (var def in root.Relations)
            {
                def.Validate();
                _relations.Add(def);
            }

            _relationsByAnchor = BuildAnchorIndex(_relations);
        }

        private static Dictionary<AnchorId, List<RelationDefinition>> BuildAnchorIndex(List<RelationDefinition> relations)
        {
            var totalParticipants = 0;
            for (int i = 0; i < relations.Count; i++)
            {
                totalParticipants += relations[i].Participants.Count;
            }

            var dict = new Dictionary<AnchorId, List<RelationDefinition>>(totalParticipants);

            for (int i=0; i<relations.Count; i++)
            {
                var relation = relations[i];
                var participants = relation.Participants;

                for (int j=0; j<participants.Count; j++)
                {
                    var anchor = participants[j].Anchor;

                    if (!dict.TryGetValue(anchor, out var list))
                    {
                        list = new List<RelationDefinition>(4);
                        dict.Add(anchor, list);
                    }

                    list.Add(relation);
                }
            }

            return dict;
        }

        public IReadOnlyList<RelationDefinition> GetAll() => _relations;

        public IReadOnlyList<RelationDefinition> FindByAnchor(AnchorId anchor)
        {
            if (_relationsByAnchor.TryGetValue(anchor, out var list))
            {
                return list;
            }

            return Array.Empty<RelationDefinition>();
        }

        public IReadOnlyList<RelationDefinition> FindByAnchors(AnchorId a, AnchorId b)
        {
            if (!_relationsByAnchor.TryGetValue(a, out var listA) ||
                !_relationsByAnchor.TryGetValue(b, out var listB))
            {
                return Array.Empty<RelationDefinition>();
            }

            if (ReferenceEquals(listA, listB))
            {
                return listA;
            }

            if (listA.Count > listB.Count)
            {
                var tmp = listA;
                listA = listB;
                listB = tmp;
            }

            var hashSet = new HashSet<RelationDefinition>(listA);
            var result = new List<RelationDefinition>(listA.Count);

            foreach (var rel in listB)
            {
                if (hashSet.Contains(rel))
                {
                    result.Add(rel);
                }
            }

            return result;
        }
    }
}

