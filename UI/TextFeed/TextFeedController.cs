using System;
using Godot;
using Neuma.Core.Logging;
using Neuma.Core.TextFeed;
using Neuma.Infrastructure;
using Neuma.Infrastructure.TextFeed;

namespace Neuma.UI.TextFeed
{
    public partial class TextFeedController : Control, ITextFeedController
    {
        private enum FeedState
        {
            Idle,
            Typing,
            Completed
        }

        [Export]
        public NodePath ContentLabelPath { get; set; }
        [Export]
        public NodePath SpeakerLabelPath { get; set; }

        private Label _contentLabel;
        private Label _speakerLabel;

        private ITextFeedService _textFeedService;
        private ITextFeedStyler _styler;
        private ITypewriter _typewriter;

        private FeedState _state = FeedState.Idle;
        private TextFeedMessage _currentMessage;
        private TextFeedVisualStyle _currentStyle;

        private const string LogCategory = "UI.TextFeed";

        public bool IsTyping => _state == FeedState.Typing;

        public override void _Ready()
        {
            _contentLabel = GetNodeOrNull<Label>(ContentLabelPath);
            if (_contentLabel == null)
            {
                Log.Error("TextFeedController: ContentLabelPath is not assigned or node not found.", null,
                    LogCategory, SceneFilePath, GetPath().ToString());
            }

            _speakerLabel = GetNodeOrNull<Label>(SpeakerLabelPath);

            if (SpeakerLabelPath != null && _speakerLabel == null)
            {
                Log.Warn("TextFeedController: SpeakerLabelPath is set, but node not found.", null,
                    LogCategory, SceneFilePath, GetPath().ToString());
            }

            _textFeedService = GameServices.GetService<ITextFeedService>();
            _styler = GameServices.GetService<ITextFeedStyler>();

            if (_textFeedService == null)
            {
                Log.Fatal("TextFeedController: ITextFeedService not available from GameServices.", null,
                    LogCategory, SceneFilePath, GetPath().ToString());
            }

            if (_styler == null)
            {
                Log.Fatal("TextFeedController: ITextFeedStyler not available from GameServices", null,
                    LogCategory, SceneFilePath, GetPath().ToString());
            }

            _typewriter = new Typewriter();

            if (_textFeedService != null)
            {
                _textFeedService.OnMessageEnqueued += OnMessageEnqueued;
            }

            var proxy = GameServices.GetService<TextFeedControllerProxy>();
            if (proxy != null)
            {
                proxy.RealController = this;
                Log.Info("TextFeedController registered itself to proxy.", null, LogCategory);
            }
            else
            {
                Log.Error("Failed to resolve TextFeedControllerProxy!", null, LogCategory);
            }

            TryStartNextMessage();
        }

        public override void _ExitTree()
        {
            if (_textFeedService != null)
            {
                _textFeedService.OnMessageEnqueued -= OnMessageEnqueued;
            }

            try
            {
                if (GameServices.IsInitialized)
                {
                    var proxy = GameServices.GetService<TextFeedControllerProxy>();
                    if (proxy.RealController == this)
                    {
                        proxy.RealController = null;
                    }
                }
            }
            catch { }
        }

        public override void _Process(double delta)
        {
            if (_typewriter != null && _typewriter.IsRunning)
            {
                _typewriter.Update((float)delta);
            }
        }

        private void OnMessageEnqueued(object sender, TextFeedMessageEventArgs args)
        {
            if (_state == FeedState.Idle)
            {
                TryStartNextMessage();
            }
        }

        private void TryStartNextMessage()
        {
            if (_textFeedService == null || _state != FeedState.Idle)
            {
                return;
            }

            var next = _textFeedService.TryDequeueNext();
            if (next == null)
            {
                return;
            }

            StartMessage(next);
        }

        private void StartMessage(TextFeedMessage message)
        {
            _currentMessage = message;
            _currentStyle = _styler.ResolveStyle(message);
            var displayText = _styler.PrepareText(message);

            ApplyStyleToUI(_currentStyle, message);

            _state = FeedState.Typing;

            var cps = ComputeTypingSpeed(message);
            _typewriter.Start(displayText, cps, OnTypewriterTextChanged, OnTypewriterCompleted);
        }

        private void OnTypewriterTextChanged(string currentText)
        {
            if (_contentLabel != null)
            {
                _contentLabel.Text = currentText;
            }
        }

        private void OnTypewriterCompleted()
        {
            _state = FeedState.Completed;

            if (_currentMessage != null && _currentMessage.AutoAdvance)
            {
                _state = FeedState.Idle;
                TryStartNextMessage();
            }
        }

        public void SkipOrAdvance()
        {
            if (_state == FeedState.Typing)
            {
                _typewriter.SkipToEnd();
                return;
            }

            if (_state == FeedState.Completed)
            {
                _state = FeedState.Idle;
                TryStartNextMessage();
            }
        }

        private void ApplyStyleToUI(TextFeedVisualStyle style, TextFeedMessage msg)
        {
            if (_speakerLabel != null)
            {
                if (style.UseSpeakerLabel && !string.IsNullOrWhiteSpace(msg.SpeakerName))
                {
                    _speakerLabel.Visible = true;
                    _speakerLabel.Text = msg.SpeakerName;
                }
                else
                {
                    _speakerLabel.Visible = false;
                    _speakerLabel.Text = string.Empty;
                }
            }
        }

        private float ComputeTypingSpeed(TextFeedMessage msg)
        {
            if (msg.TypingSpeedOverride.HasValue)
            {
                return Math.Max(1.0f, msg.TypingSpeedOverride.Value);
            }

            var baseSpeed = Math.Max(1.0f, Settings.Current.TextFeedTypingSpeed);

            var multiplier = msg.Mode switch
            {
                TextFeedMode.SystemMessage => 1.5f,
                TextFeedMode.Caption => 0.8f,
                TextFeedMode.Notification => 1.2f,
                _ => 1.0f
            };

            return baseSpeed * multiplier;
        }
    }
}

