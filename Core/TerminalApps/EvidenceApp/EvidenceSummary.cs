using Neuma.Core.EvidenceSystem;

namespace Neuma.Core.TerminalApps.EvidenceApp
{
    public record class EvidenceSummary(string Id, string Title, EvidenceType Type, bool IsNew);
}
