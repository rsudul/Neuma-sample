using System;

namespace Neuma.Core.Cases
{
    public sealed class CaseStartedEventArgs : EventArgs
    {
        public string CaseId { get; }
        public CaseDefinition Definition { get; }

        public CaseStartedEventArgs(string caseId, CaseDefinition definition)
        {
            CaseId = caseId ?? throw new ArgumentNullException(nameof(caseId));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }
    }
}

