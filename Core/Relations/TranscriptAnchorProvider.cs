using System;

namespace Neuma.Core.Relations
{
    public sealed class TranscriptAnchorProvider : ILinkAnchorProvider
    {
        //private readonly ITranscriptSystem _transcriptSystem;

        public AnchorSourceType SourceType => AnchorSourceType.Transcript;

        public TranscriptAnchorProvider(/*ITranscriptSystem transcriptSystem*/)
        {
            //_transcriptSystem = transcriptSystem ?? throw new ArgumentNullException(nameof(transcriptSystem));
        }

        public bool TryGetAnchorId(string caseId, string objectId, string? subId, out AnchorId anchorId)
        {
            anchorId = default;

            if (string.IsNullOrWhiteSpace(caseId) || string.IsNullOrWhiteSpace(objectId))
            {
                return false;
            }

            //if (!_transcriptSystem.TranscriptLineExists(objectId)) return false;

            anchorId = new AnchorId(caseId, AnchorSourceType.Transcript, objectId, subId);

            return true;
        }
    }
}

