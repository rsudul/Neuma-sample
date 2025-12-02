using System;
using System.Collections.Generic;

namespace Neuma.Core.Relations
{
    [System.Serializable]
    public sealed class RelationDefinition
    {
        public string RelationId { get; }
        public string CaseId { get; }
        public RelationType RelationType { get; }
        public IReadOnlyList<RelationParticipant> Participants { get; }
        public RelationMetadata? Metadata { get; }

        public RelationDefinition(string relationId, string caseId, RelationType relationType,
            IReadOnlyList<RelationParticipant> participants, RelationMetadata? metadata = null)
        {
            if (string.IsNullOrWhiteSpace(relationId))
            {
                throw new ArgumentException("RelationId cannot be null or whitespace.", nameof(relationId));
            }

            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            if (participants == null)
            {
                throw new ArgumentNullException(nameof(participants));
            }

            if (participants.Count < 2)
            {
                throw new ArgumentException("RelationDefinition must have at least two participants.", nameof(participants));
            }

            RelationId = relationId;
            CaseId = caseId;
            RelationType = relationType;
            Participants = participants;
            Metadata = metadata;
        }

        public void Validate()
        {
            if (Participants.Count < 2)
            {
                throw new InvalidOperationException($"Relation '{RelationId}' must have at least two participants.");
            }

            foreach (var participant in Participants)
            {
                if (participant.Anchor.CaseId != CaseId)
                {
                    throw new InvalidOperationException(
                        $"Relation '{RelationId}' has participant with mismatched CaseId: {participant.Anchor.CaseId} (expected {CaseId}).");
                }
            }
        }

        public override string ToString()
        {
            return $"{RelationId} ({CaseId}, {RelationType}, participants: {Participants.Count})";
        }
    }
}

