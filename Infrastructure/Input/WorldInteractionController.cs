using System;
using Godot;
using Neuma.Core.Input;
using Neuma.Core.Logging;
using Neuma.Core.TerminalFeed;

namespace Neuma.Infrastructure.Input
{
    /// <summary>
    /// Responsible for detecting what player is looking at, through raycasting.
    /// Handles multiple cameras assigned to different UI areas (feeds).
    /// Translates physical in-game object into InputContext and sends that info to InputEventRouter.
    /// </summary>
    public partial class WorldInteractionController : Node
    {
        [Serializable]
        public partial class FeedMapping : Resource
        {
            [Export] public NodePath UIFramePath { get; set; }
            [Export] public NodePath CameraPath { get; set; }
            public Control UIFrame { get; set; }
            public Camera3D Camera { get; set; }
        }

        private InputEventRouter _router;
        private IInputContextProvider? _lastProvider;

        private ITerminalController _terminalController;

        private const string LogCategory = "Infrastructure.Input.WorldInteraction";

        [Export]
        public Godot.Collections.Array<FeedMapping> Feeds { get; set; } = new();

        [Export(PropertyHint.Layers3DPhysics)]
        public uint InteractionMask { get; set; } = 1;

        [Export]
        public float RayLength { get; set; } = 1000.0f;

        public override void _Ready()
        {
            if (GameServices.IsInitialized)
            {
                _router = GameServices.GetService<InputEventRouter>();
                _terminalController = GameServices.GetService<ITerminalController>();
            }
            else
            {
                Log.Error("GameServices not initialized in WorldInteractionController.", null, LogCategory);
                SetPhysicsProcess(false);
                return;
            }

            foreach (var mapping in Feeds)
            {
                if (mapping.UIFramePath != null)
                {
                    mapping.UIFrame = GetNodeOrNull<Control>(mapping.UIFramePath);
                    if (mapping.UIFrame == null)
                    {
                        Log.Warn($"UIFrame node not found at path: {mapping.UIFramePath}.", null, LogCategory);
                    }
                }

                if (mapping.CameraPath != null)
                {
                    mapping.Camera = GetNodeOrNull<Camera3D>(mapping.CameraPath);
                    if (mapping.Camera == null)
                    {
                        Log.Warn($"Camera node not found at path: {mapping.CameraPath}.", null, LogCategory);
                    }
                }
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_router == null)
            {
                return;
            }

            if (_router.ActiveContext == InputContext.Menu)
            {
                return;
            }

            var mousePos = GetViewport().GetMousePosition();

            var activeMapping = FindActiveFeed(mousePos);
            if (activeMapping == null)
            {
                ClearFocus();
                return;
            }

            var (provider, hitPoint) = RaycastFromCamera(activeMapping.Camera, mousePos, activeMapping.UIFrame);

            if (provider != null && provider.IsInteractive)
            {
                UpdateFocus(provider);

                if (provider.Context == InputContext.Gameplay && provider is InputContextSource source)
                {
                    UpdateVirtualCursor(source, hitPoint);
                }
            }
            else
            {
                ClearFocus();
            }
        }

        private FeedMapping? FindActiveFeed(Vector2 mousePos)
        {
            foreach (var mapping in Feeds)
            {
                if (mapping.UIFrame == null || !mapping.UIFrame.IsVisibleInTree())
                {
                    continue;
                }

                if (mapping.UIFrame.GetGlobalRect().HasPoint(mousePos))
                {
                    return mapping;
                }
            }

            return null;
        }

        private (IInputContextProvider?, Vector3) RaycastFromCamera(Camera3D camera, Vector2 globalMousePos, Control uiFrame)
        {
            if (camera == null)
            {
                return (null, Vector3.Zero);
            }

            var from = camera.ProjectRayOrigin(globalMousePos);
            var dir = camera.ProjectRayNormal(globalMousePos);
            var to = from + dir * RayLength;

            var spaceState = camera.GetWorld3D().DirectSpaceState;
            var query = PhysicsRayQueryParameters3D.Create(from, to, InteractionMask);
            query.CollideWithAreas = true;
            query.CollideWithBodies = true;

            var result = spaceState.IntersectRay(query);

            if (result.Count == 0)
            {
                return (null, Vector3.Zero);
            }

            var hitPosition = result["position"].AsVector3();
            var collider = result["collider"].As<Node>();

            if (TryFindProvider(collider, out var foundProvider))
            {
                return (foundProvider, hitPosition);
            }

            return (null, Vector3.Zero);
        }

        private void UpdateFocus(IInputContextProvider provider)
        {
            if (_lastProvider == provider)
            {
                return;
            }

            _lastProvider = provider;
            _router.SetActiveContext(provider.Context);
        }

        private void ClearFocus()
        {
            if (_lastProvider == null)
            {
                return;
            }

            _lastProvider = null;

            _router.SetActiveContext(InputContext.None);
        }

        private bool TryFindProvider(Node collider, out IInputContextProvider? provider)
        {
            provider = null;

            if (collider is IInputContextProvider p)
            {
                provider = p;
                return true;
            }

            foreach (var c in collider.GetChildren())
            {
                if (c is IInputContextProvider p2)
                {
                    provider = p2;
                    return true;
                }
            }

            var parent = collider.GetParent();

            if (parent is IInputContextProvider p3)
            {
                provider = p3;
                return true;
            }

            if (parent == null)
            {
                return false;
            }

            foreach (var c in parent.GetChildren())
            {
                if (c is IInputContextProvider p4)
                {
                    provider = p4;
                    return true;
                }
            }

            return false;
        }

        private void UpdateVirtualCursor(InputContextSource source, Vector3 globalHitPoint)
        {
            if (_terminalController == null)
            {
                return;
            }

            var node3d = source.GetParent() as Node3D;
            if (node3d == null)
            {
                return;
            }

            var localPoint = node3d.ToLocal(globalHitPoint);

            float width = source.SurfaceSize.X;
            float height = source.SurfaceSize.Y;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            float u = (localPoint.X / width) + 0.5f;
            float v = 0.5f - (localPoint.Y / height);

            u = Mathf.Clamp(u, 0.0f, 1.0f);
            v = Mathf.Clamp(v, 0.0f, 1.0f);

            _terminalController.ProcessPointer(new Vector2(u, v), false);
        }
    }
}

