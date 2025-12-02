using System;
using Godot;
using Neuma.Core.Logging;
using Neuma.Core.TerminalFeed;
using Neuma.Core.TextFeed;

namespace Neuma.Core.Input
{
    public sealed class GameplayInputHandler : IInputHandler
    {
        private readonly ITerminalController _terminalController;
        private readonly ITextFeedController _textFeedController;

        private const string LogCategory = "Core.Input.Gameplay";

        public GameplayInputHandler(ITerminalController terminalController, ITextFeedController textFeedController)
        {
            _terminalController = terminalController ?? throw new ArgumentNullException(nameof(terminalController));
            _textFeedController = textFeedController ?? throw new ArgumentNullException(nameof(textFeedController));
        }

        public void OnFocusGained()
        {
            Log.Debug("Gameplay input focus gained.", null, LogCategory);
        }

        public void OnFocusLost()
        {
            Log.Debug("Gameplay input focus lost.", null, LogCategory);
        }

        public bool HandleInput(InputEvent @event)
        {
            bool isConfirmAction = @event.IsActionPressed("ui_accept");
            bool isClickAction = (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left);

            if (isConfirmAction || isClickAction)
            {
                if (_textFeedController.IsTyping)
                {
                    _textFeedController.SkipOrAdvance();
                    return true;
                }
            }

            if (@event.IsActionPressed("ui_up"))
            {
                _terminalController.ProcessInput(TerminalCommand.NavigateUp);
                return true;
            }
            if (@event.IsActionPressed("ui_down"))
            {
                _terminalController.ProcessInput(TerminalCommand.NavigateDown);
                return true;
            }
            if (@event.IsActionPressed("ui_left"))
            {
                _terminalController.ProcessInput(TerminalCommand.NavigateLeft);
                return true;
            }
            if (@event.IsActionPressed("ui_right"))
            {
                _terminalController.ProcessInput(TerminalCommand.NavigateRight);
                return true;
            }
            if (isConfirmAction)
            {
                _terminalController.ProcessInput(TerminalCommand.Confirm);
                return true;
            }
            if (@event.IsActionPressed("ui_cancel"))
            {
                _terminalController.ProcessInput(TerminalCommand.Cancel);
                return true;
            }

            if (isClickAction)
            {
                _terminalController.ProcessInput(TerminalCommand.Confirm);
                return true;
            }

            if (@event is InputEventMouseButton mouseRight
                && mouseRight.Pressed
                && mouseRight.ButtonIndex == MouseButton.Right)
            {
                _terminalController.ProcessInput(TerminalCommand.Cancel);
                return true;
            }

            return false;
        }
    }
}

