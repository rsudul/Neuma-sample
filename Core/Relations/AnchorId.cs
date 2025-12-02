using System;

namespace Neuma.Core.Relations
{
    [Serializable]
    public readonly struct AnchorId : IEquatable<AnchorId>
    {
        public string CaseId { get; }
        public AnchorSourceType SourceType { get; }
        public string ObjectId { get; }
        public string? SubId { get; }

        public AnchorId(string caseId, AnchorSourceType sourceType, string objectId, string? subId = null)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            if (string.IsNullOrWhiteSpace(objectId))
            {
                throw new ArgumentException("ObjectId cannot be null or whitespace.", nameof(objectId));
            }

            CaseId = caseId;
            SourceType = sourceType;
            ObjectId = objectId;
            SubId = string.IsNullOrWhiteSpace(subId) ? null : subId;
        }

        public static AnchorId ForEvidence(string caseId, string evidenceId, string? subId = null)
        {
            return new AnchorId(caseId, AnchorSourceType.Evidence, evidenceId, subId);
        }

        public static AnchorId ForTranscript(string caseId, string transcriptLineId, string? subId = null)
        {
            return new AnchorId(caseId, AnchorSourceType.Transcript, transcriptLineId, subId);
        }

        public bool Equals(AnchorId other)
        {
            return CaseId == other.CaseId
                && SourceType == other.SourceType
                && ObjectId == other.ObjectId
                && SubId == other.SubId;
        }

        public override bool Equals(object? obj)
        {
            return obj is AnchorId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CaseId, SourceType, ObjectId, SubId);
        }

        public static bool operator ==(AnchorId left, AnchorId right) => left.Equals(right);
        public static bool operator !=(AnchorId left, AnchorId right) => !left.Equals(right);

        public override string ToString()
        {
            var sub = SubId ?? "-";
            return $"{CaseId}|{SourceType}|{ObjectId}|{sub}";
        }
    }
}

