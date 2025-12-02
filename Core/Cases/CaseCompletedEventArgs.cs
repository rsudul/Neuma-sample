using System;

namespace Neuma.Core.Cases
{
    public sealed class CaseCompletedEventArgs : EventArgs
    {
        public string CaseId { get; }

        public CaseCompletedEventArgs(string caseId)
        {
            CaseId = caseId ?? throw new ArgumentNullException(nameof(caseId));
        }
    }
}

