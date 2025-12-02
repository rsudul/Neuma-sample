using System;

namespace Neuma.Core.EvidenceSystem
{
    public sealed class EvidenceUnlockedEventArgs : EventArgs
    {
        public string CaseId { get; }
        public string EvidenceId { get; }
        public bool IsSilent { get; }

        public EvidenceUnlockedEventArgs(string caseId, string evidenceId, bool isSilent = false)
        {
            CaseId = caseId;
            EvidenceId = evidenceId;
            IsSilent = isSilent;
        }
    }
}

