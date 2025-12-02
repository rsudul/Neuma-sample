using System;

namespace Neuma.Core.Relations
{
    public sealed class AnchorSelectedEventArgs : EventArgs
    {
        public AnchorId Anchor { get; }
        public string DisplayName { get; }

        public AnchorSelectedEventArgs(AnchorId anchor, string displayName)
        {
            Anchor = anchor;
            DisplayName = displayName;
        }
    }
}

