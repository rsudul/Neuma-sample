using Godot;
using Neuma.Core.Input;

namespace Neuma.Infrastructure.Input
{
    /// <summary>
    /// This class lets you define what InputContext the object it's attached to, has.
    /// For example, Terminal object -> Gameplay context.
    /// </summary>
    public partial class InputContextSource : Node, IInputContextProvider
    {
        [Export]
        public InputContext Context { get; set; } = InputContext.Gameplay;
        [Export]
        public bool IsInteractive { get; set; } = true;
        [Export]
        public Vector2 SurfaceSize { get; set; } = new Vector2(1.0f, 1.0f);
    }
}

