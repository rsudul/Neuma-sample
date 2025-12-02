using System;
using Godot;
using Neuma.Core.Feeds;
using Neuma.Core.Logging;

namespace Neuma.Core.HologramFeed
{
    public partial class HologramFeedController : Node3D, IFeed
    {
        public enum HologramState
        {
            Booting,
            IdleDisconnected,
            Connecting,
            ConnectedPlayingDialogue,
            ConnectedIdle,
            SessionEnded
        }

        private HologramState _state;

        private const string LogCategory = "Core.HologramFeed";

        private bool _indicatorShouldBlink = false;
        private double _indicatorBlinkTimer = 0.0f;
        private bool _indicatorVisibleState = false;

        [Export] public MeshInstance3D ProjectorMesh { get; set; }
        [Export] public Area3D ProjectorButton { get; set; }
        [Export] public Light3D IndicatorLight { get; set; }
        [Export] public MeshInstance3D HologramSubject { get; set; }
        [Export] public Camera3D HologramCamera { get; set; }

        [Export] public float BootingDurationSeconds { get; set; } = 0.3f;
        [Export] public float ConnectingDurationSeconds { get; set; } = 1.5f;
        [Export] public float IndicatorBlinkIntervalSeconds { get; set; } = 0.25f;

        [Export] public float DialogueDurationSeconds { get; set; } = 4.0f;

        public event EventHandler? ProjectorActivationRequested;
        public event EventHandler? DialoguePlaybackFinished;
        public event EventHandler? SessionEndVisualsFinished;

        public FeedId Id => FeedId.Hologram;

        public override void _Ready()
        {
            SetState(HologramState.Booting);

            GetTree().CreateTimer(BootingDurationSeconds).Timeout += OnBootingFinished;
        }

        public override void _Process(double delta)
        {
            if (_indicatorShouldBlink && IndicatorLight != null)
            {
                _indicatorBlinkTimer -= delta;

                if (_indicatorBlinkTimer <= 0.0f)
                {
                    _indicatorBlinkTimer += IndicatorBlinkIntervalSeconds;
                    _indicatorVisibleState = !_indicatorVisibleState;
                    IndicatorLight.Visible = _indicatorVisibleState;
                }
            }
        }

        private void ValidateReferences()
        {
            if (HologramSubject == null)
            {
                Log.Warn("HologramSubject is not assigned in HologramFeedController.", null, LogCategory);
            }

            if (IndicatorLight == null)
            {
                Log.Warn("IndicatorLight is not assigned in HologramFeedController.", null, LogCategory);
            }

            if (ProjectorButton == null)
            {
                Log.Warn("ProjectorButton is not assigned in HologramFeedController.", null, LogCategory);
            }

            if (HologramCamera == null)
            {
                Log.Warn("HologramCamera is not assigned in HologramFeedController.", null, LogCategory);
            }
        }

        private void OnBootingFinished()
        {
            SetState(HologramState.IdleDisconnected);
        }

        private void SetState(HologramState newState)
        {
            if (_state == newState)
            {
                return;
            }

            Log.Debug($"Hologram state change: {_state} -> {newState}.", null, LogCategory);

            _state = newState;

            switch (newState)
            {
                case HologramState.Booting:
                    EnterBooting();
                    break;

                case HologramState.IdleDisconnected:
                    EnterIdleDisconnected();
                    break;

                case HologramState.Connecting:
                    EnterConnecting();
                    break;

                case HologramState.ConnectedPlayingDialogue:
                    EnterConnectedPlayingDialogue();
                    break;

                case HologramState.ConnectedIdle:
                    EnterConnectedIdle();
                    break;

                case HologramState.SessionEnded:
                    EnterSessionEnded();
                    break;
            }
        }

        private void EnterBooting()
        {
            Log.Info("Entering Booting state.", null, LogCategory);

            if (HologramSubject != null)
            {
                HologramSubject.Visible = false;
            }

            if (IndicatorLight != null)
            {
                IndicatorLight.Visible = false;
            }

            _indicatorShouldBlink = false;
        }

        private void EnterIdleDisconnected()
        {
            Log.Info("Entering IdleDisconnected state (connection pending).", null, LogCategory);

            if (HologramSubject != null)
            {
                HologramSubject.Visible = false;
            }

            if (IndicatorLight != null)
            {
                IndicatorLight.Visible = false;
            }

            _indicatorShouldBlink = false;
        }

        private void EnterConnecting()
        {
            Log.Info("Entering Connecting state.", null, LogCategory);

            if (HologramSubject != null)
            {
                HologramSubject.Visible = true;
            }

            if (IndicatorLight != null)
            {
                IndicatorLight.Visible = true;
                _indicatorShouldBlink = true;
                _indicatorBlinkTimer = 0.0f;
                _indicatorVisibleState = true;
            }

            GetTree().CreateTimer(ConnectingDurationSeconds).Timeout += OnConnectingFinished;
        }

        private void EnterConnectedPlayingDialogue()
        {
            Log.Info("Entering ConnectedPlayingDialogue state.", null, LogCategory);

            if (HologramSubject != null)
            {
                HologramSubject.Visible = true;
            }

            _indicatorShouldBlink = false;
            if (IndicatorLight != null)
            {
                IndicatorLight.Visible = true;
            }

            GetTree().CreateTimer(DialogueDurationSeconds).Timeout += OnDialoguePlaybackTimerTimeout;
        }

        private void EnterConnectedIdle()
        {
            Log.Info("Entering ConnectedIdle state.", null, LogCategory);

            if (HologramSubject != null)
            {
                HologramSubject.Visible = true;
            }

            _indicatorShouldBlink = false;
            if (IndicatorLight != null)
            {
                IndicatorLight.Visible = true;
            }
        }

        private void EnterSessionEnded()
        {
            Log.Info("Entering SessionEnded state. Shutting down hologram visuals.",
                null, LogCategory);

            if (HologramSubject != null)
            {
                HologramSubject.Visible = false;
            }

            _indicatorShouldBlink = false;

            if (IndicatorLight != null)
            {
                IndicatorLight.Visible = false;
            }

            OnSessionEndVisualsFinished();
        }

        private void OnProjectorButtonInputEvent(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shapeIdx)
        {
            if (@event is not InputEventMouseButton mouseEvent)
            {
                return;
            }

            if (!mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left)
            {
                return;
            }

            if (_state == HologramState.IdleDisconnected)
            {
                Log.Debug("Projector button clicked in IdleDisconnected. Requesting activation...",
                    null, LogCategory);

                OnProjectorActivationRequested();

                SetState(HologramState.Connecting);
            }
            else
            {
                Log.Trace($"Projector button clicked in state {_state}, ignoring.", null, LogCategory);
            }
        }

        private void OnConnectingFinished()
        {
            Log.Debug("Connecting timer finished. Assuming connection established.", null, LogCategory);

            SetState(HologramState.ConnectedPlayingDialogue);
        }

        private void OnDialoguePlaybackTimerTimeout()
        {
            Log.Debug("Dialogue playback finished (timer). Switching to ConnectedIdle.",
                null, LogCategory);

            OnDialoguePlaybackFinished();

            SetState(HologramState.ConnectedIdle);
        }

        protected virtual void OnProjectorActivationRequested()
        {
            ProjectorActivationRequested?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDialoguePlaybackFinished()
        {
            DialoguePlaybackFinished?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSessionEndVisualsFinished()
        {
            SessionEndVisualsFinished?.Invoke(this, EventArgs.Empty);
        }

        public void ShowFeed()
        {
            Log.Debug("Show() called on Hologram feed.", null, LogCategory);
            Visible = true;
        }

        public void HideFeed()
        {
            Log.Debug("Hide() called on Hologram feed.", null, LogCategory);
            Visible = false;
        }
    }
}

