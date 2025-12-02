using System;

namespace Neuma.Core.Cases
{
    public class CaseStatusChangedEventArgs : EventArgs
    {
        public CaseStatus OldStatus { get; }
        public CaseStatus NewStatus { get; }

        public CaseStatusChangedEventArgs(CaseStatus oldStatus, CaseStatus newStatus)
        {
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }
}

