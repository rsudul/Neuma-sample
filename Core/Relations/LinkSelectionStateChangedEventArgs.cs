using System;

namespace Neuma.Core.Relations
{
    public sealed class LinkSelectionStateChangedEventArgs : EventArgs
    {
        public LinkSelectionViewContext State { get; }

        public LinkSelectionStateChangedEventArgs(LinkSelectionViewContext state)
        {
            State = state;
        }
    }
}

