using System;

namespace Neuma.Core.Input
{
    public interface IInputService
    {
        bool IsInputLocked { get; }
        InputMode CurrentMode { get; }

        event EventHandler<InputMode> OnInputModeChanged;

        void LockInput();
        void UnlockInput();
        void SetInputMode(InputMode mode);
    }
}

