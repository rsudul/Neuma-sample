using System.Collections.Generic;

namespace Neuma.Core.TranscriptSystem
{
    /// <summary>
    /// Stores runtime transcript history for all interrogations that actually happened in the game.
    /// This is separate from ITranscriptRepository/IDialoguesRepository which provides static,
    /// pre-authored data.
    /// </summary>
    public interface ITranscriptHistoryRepository
    {
        IReadOnlyList<TranscriptLine> GetAllLines(string caseId, string transcriptId);
        IReadOnlyList<TranscriptLine> GetAllLinesForCase(string caseId);
        void AppendLine(TranscriptLine line);
        void ClearTranscript(string caseId, string transcriptId);
    }
}

