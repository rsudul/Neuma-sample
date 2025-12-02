using Godot;

namespace Neuma.Core.Input
{
    /// <summary>
    /// Interface for components able to handle redirected user input.
    /// Implemented by adapters of specific systems (e.g., TerminalInputHandler).
    /// </summary>
    public interface IInputHandler
    {
        bool HandleInput(InputEvent @event);
        void OnFocusGained();
        void OnFocusLost();
    }
}

