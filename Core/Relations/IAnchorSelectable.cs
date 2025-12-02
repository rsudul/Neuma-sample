using System;

namespace Neuma.Core.Relations
{
    /// <summary>
    /// Interface to implement by classes allowing selecting elements for pair matching.
    /// </summary>
    public interface IAnchorSelectable
    {
        event EventHandler<AnchorSelectedEventArgs> OnAnchorSelected;
    }
}

