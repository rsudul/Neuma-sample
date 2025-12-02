using System;
using Godot;

namespace Neuma.Core.TerminalFeed
{
    public interface ITerminalController
    {
        ITerminalApp CurrentApp { get; }

        void OpenApp(string appId);
        void CloseCurrentApp();
        void ProcessInput(TerminalCommand command);
        void ProcessPointer(Vector2 normalizedPosition, bool isClick);

        event EventHandler OnAppChanged;
        event EventHandler OnStateChanged;
    }
}

