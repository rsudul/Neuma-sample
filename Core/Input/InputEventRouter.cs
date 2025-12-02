using System;
using System.Collections.Generic;
using Godot;
using Neuma.Core.Logging;

namespace Neuma.Core.Input
{
    /// <summary>
    /// The central point of input events distribution.
    /// Decides, which system (via a handler) should receive input, based on the active context
    /// (from InputContext, e.g. Gameplay or Menu).
    /// Inherits from Node, so it has access to _UnhandledInput.
    /// </summary>
    public partial class InputEventRouter : Node
    {
        private readonly IInputService _inputService;
        private readonly Dictionary<InputContext, IInputHandler> _handlers = new();

        private IInputHandler? _activeHandler;
        private InputContext _activeContext = InputContext.None;

        private const string LogCategory = "Core.Input.Router";

        public InputContext ActiveContext => _activeContext;

        public InputEventRouter(IInputService inputService)
        {
            _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        }

        public void RegisterHandler(InputContext context, IInputHandler handler)
        {
            if (handler == null)
            {
                Log.Warn($"Attempted to register null handler for Context: {context}.", null, LogCategory);
                return;
            }

            if (_handlers.ContainsKey(context))
            {
                Log.Warn($"Handler for Context {context} is already registered. Overwriting.", null, LogCategory);
            }

            _handlers[context] = handler;
            Log.Debug($"Registered input handler for Context: {context}.", null, LogCategory);
        }

        public void SetActiveContext(InputContext context)
        {
            if (_activeContext == context)
            {
                return;
            }

            if (context == InputContext.None)
            {
                ChangeActiveHandler(null, context);
                return;
            }

            if (!_handlers.TryGetValue(context, out var newHandler))
            {
                Log.Warn($"Requested switch to Context {context}, but no handler is registered.", null, LogCategory);
                ChangeActiveHandler(null, InputContext.None);
                return;
            }

            ChangeActiveHandler(newHandler, context);
        }

        private void ChangeActiveHandler(IInputHandler? newHandler, InputContext newContext)
        {
            if (_activeHandler != null)
            {
                _activeHandler.OnFocusLost();
            }

            _activeHandler = newHandler;
            _activeContext = newContext;

            if (_activeHandler != null)
            {
                _activeHandler.OnFocusGained();
                Log.Debug($"Input context changed to: {newContext} ({_activeHandler.GetType().Name}).", null, LogCategory);
            }
            else
            {
                Log.Debug("Input context cleared.", null, LogCategory);
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (_inputService.IsInputLocked)
            {
                return;
            }

            if (@event.IsActionPressed("ui_cancel"))
            {
                if (_activeContext == InputContext.Menu)
                {
                    SetActiveContext(InputContext.None);
                }
                else
                {
                    SetActiveContext(InputContext.Menu);
                }

                GetViewport().SetInputAsHandled();
                return;
            }

            if (_activeHandler != null)
            {
                bool consumed = _activeHandler.HandleInput(@event);
                if (consumed)
                {
                    GetViewport().SetInputAsHandled();
                }
            }
        }

        public override void _ExitTree()
        {
            _activeHandler?.OnFocusLost();
            _activeHandler = null;
            _handlers.Clear();
        }
    }
}

