using System;

namespace Neuma.Core.Relations
{
    public sealed class EvidenceAnchorProvider : ILinkAnchorProvider
    {
        //private readonly IEvidenceSystem _evidenceSystem;

        public AnchorSourceType SourceType => AnchorSourceType.Evidence;

        public EvidenceAnchorProvider(/*IEvidenceSystem evidenceSystem*/)
        {
            //_evidenceSystem = evidenceSystem ?? throw new ArgumentNullException(nameof(evidenceSystem));
        }

        public bool TryGetAnchorId(string caseId, string objectId, string? subId, out AnchorId anchorId)
        {
            anchorId = default;

            if (string.IsNullOrWhiteSpace(caseId) || string.IsNullOrWhiteSpace(objectId))
            {
                return false;
            }

            // later check with real evidence system
            /*if (!_evidenceSystem.EvidenceExists(objectId))
            {
                return false;
            }*/

            anchorId = new AnchorId(caseId, AnchorSourceType.Evidence, objectId, subId);

            return true;
        }
    }
}

