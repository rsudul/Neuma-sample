using System;
using Neuma.Core.Logging;

namespace Neuma.Core.Input
{
    public sealed class InputService : IInputService
    {
        private bool _isLocked;
        private InputMode _currentMode = InputMode.MouseAndKeyboard;

        private const string LogCategory = "Core.Input";

        public bool IsInputLocked => _isLocked;
        public InputMode CurrentMode => _currentMode;

        public event EventHandler<InputMode>? OnInputModeChanged;

        public void LockInput()
        {
            if (!_isLocked)
            {
                _isLocked = true;
                Log.Info("Input locked globally.", null, LogCategory);
            }
        }

        public void UnlockInput()
        {
            if (_isLocked)
            {
                _isLocked = false;
                Log.Info("Input unlocked globally.", null, LogCategory);
            }
        }

        public void SetInputMode(InputMode mode)
        {
            if (_currentMode != mode)
            {
                _currentMode = mode;
                Log.Info($"Input mode changed to: {mode}.", null, LogCategory);
                OnInputModeChanged?.Invoke(this, mode);
            }
        }
    }
}

