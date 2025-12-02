using System;
using Godot;

namespace Neuma.Core.TerminalFeed
{
    public interface ITerminalApp
    {
        string AppId { get; }

        void HandleCommand(TerminalCommand command);
        void HandlePointer(Vector2 normalizedPosition, bool isClick);
        ITerminalViewContext GetCurrentContext();
        void OnOpen();
        void OnClose();

        event EventHandler<TerminalAppStateChangedEventArgs> OnStateChanged;
    }
}

