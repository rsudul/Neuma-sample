using System.Collections.Generic;
using Neuma.Core.TerminalFeed;

namespace Neuma.Core.TerminalApps.TranscriptApp
{
    public record class TranscriptAppViewContext(string CaseId, IReadOnlyList<TranscriptSessionSummary> Sessions,
        int SelectedIndex, IReadOnlyList<TranscriptLineView> CurrentTranscriptLines,
        int? SelectedLineIndex) : ITerminalViewContext;
}

