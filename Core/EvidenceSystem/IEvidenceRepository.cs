using System.Collections.Generic;

namespace Neuma.Core.EvidenceSystem
{
    public interface IEvidenceRepository
    {
        IReadOnlyList<Evidence> GetAll(string caseId);
        bool TryGet(string caseId, string evidenceId, out Evidence? evidence);
        Evidence Get(string caseId, string evidenceId);
    }
}

