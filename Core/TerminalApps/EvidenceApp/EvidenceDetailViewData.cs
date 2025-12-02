using System.Collections.Generic;

namespace Neuma.Core.TerminalApps.EvidenceApp
{
    public record class EvidenceDetailViewData(string Id, string Title, string Description, string AssetPath,
        IReadOnlyDictionary<string, string> Metadata, IReadOnlyList<string> Tags);
}

