using System;

namespace Neuma.Core.TerminalFeed
{
    public sealed class TerminalAppStateChangedEventArgs : EventArgs
    {
        public ITerminalViewContext OldState { get; }
        public ITerminalViewContext NewState { get; }

        public TerminalAppStateChangedEventArgs(ITerminalViewContext oldState, ITerminalViewContext newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }
}

