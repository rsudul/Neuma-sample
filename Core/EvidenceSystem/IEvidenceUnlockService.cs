using System;

namespace Neuma.Core.EvidenceSystem
{
    public interface IEvidenceUnlockService
    {
        bool IsDiscovered(string caseId, string evidenceId);
        bool IsNew(string caseId, string evidenceId);

        void UnlockEvidence(string caseId, string evidenceId);
        void MarkAsRead(string caseId, string evidenceId);

        event EventHandler<EvidenceUnlockedEventArgs> OnEvidenceUnlocked;
    }
}

