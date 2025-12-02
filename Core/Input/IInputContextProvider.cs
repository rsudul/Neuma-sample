namespace Neuma.Core.Input
{
    /// <summary>
    /// Interface for in-game objects (Node3D/Colliders) which change input context
    /// when player interacts with them.
    /// </summary>
    public interface IInputContextProvider
    {
        InputContext Context { get; }
        bool IsInteractive { get; }
    }
}

