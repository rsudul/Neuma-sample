using System;

namespace Neuma.Core.Relations
{
    public interface ILinkAnchorProvider
    {
        AnchorSourceType SourceType { get; }
        bool TryGetAnchorId(string caseId, string objectId, string? subId, out AnchorId anchorId);
    }
}

