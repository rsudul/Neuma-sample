using System.Collections.Generic;
using Neuma.Core.TerminalFeed;

namespace Neuma.Core.TerminalApps.EvidenceApp
{
    public record class EvidenceAppViewContext(string CaseId, IReadOnlyList<EvidenceSummary> Items,
        int SelectedIndex, EvidenceDetailViewData? DetailView) : ITerminalViewContext;
}

