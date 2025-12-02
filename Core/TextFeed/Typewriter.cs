using System;
using System.Text;
using Neuma.Core.Logging;

namespace Neuma.Core.TextFeed
{
    public sealed class Typewriter : ITypewriter
    {
        private string _fullText = string.Empty;
        private readonly StringBuilder _buffer = new StringBuilder(256);

        private float _charsPerSecond;
        private float _accumulator;
        private int _currentVisibleLength;

        private Action<string>? _onTextChanged;
        private Action? _onCompleted;
        private bool _completedCallbackInvoked;

        private const string LogCategory = "Core.TextFeed.Typewriter";

        public bool IsRunning { get; private set; }

        public float Progress
        {
            get
            {
                if (!IsRunning && _fullText.Length == 0)
                {
                    return 0.0f;
                }

                if (_fullText.Length == 0)
                {
                    return 1.0f;
                }

                return Math.Clamp((float)_currentVisibleLength / _fullText.Length, 0.0f, 1.0f);
            }
        }

        public void Start(string fullText, float charsPerSecond, Action<string> onTextChanged, Action onCompleted)
        {
            if (fullText == null)
            {
                throw new ArgumentNullException(nameof(fullText));
            }
            if (onTextChanged == null)
            {
                throw new ArgumentNullException(nameof(onTextChanged));
            }
            if (onCompleted == null)
            {
                throw new ArgumentNullException(nameof(onCompleted));
            }
            if (charsPerSecond <= 0.0f)
            {
                Log.Error($"Typewriter.Start() called with non-positive charsPerSecond={charsPerSecond}.",
                    null, LogCategory);
                throw new ArgumentOutOfRangeException(nameof(charsPerSecond), "Typing speed must be > 0.");
            }

            _fullText = fullText;
            _charsPerSecond = charsPerSecond;
            _onTextChanged = onTextChanged;
            _onCompleted = onCompleted;

            _accumulator = 0.0f;
            _currentVisibleLength = 0;
            _completedCallbackInvoked = false;
            IsRunning = true;

            _buffer.Clear();
            _onTextChanged(string.Empty);

            if (_fullText.Length == 0)
            {
                CompleteImmediately();
            }
        }

        public void Update(float deltaSeconds)
        {
            if (!IsRunning)
            {
                return;
            }

            if (deltaSeconds <= 0.0f || _fullText.Length == 0)
            {
                return;
            }

            _accumulator += deltaSeconds;

            var charsToRevealFloat = _charsPerSecond * _accumulator;
            var charsToReveal = (int)Math.Floor(charsToRevealFloat);

            if (charsToReveal <= 0)
            {
                return;
            }

            _accumulator -= charsToReveal / _charsPerSecond;

            var targetVisibleLength = _currentVisibleLength + charsToReveal;
            if (targetVisibleLength >= _fullText.Length)
            {
                _currentVisibleLength = _fullText.Length;
                EmitCurrentText();

                Finish();
                return;
            }

            _currentVisibleLength = targetVisibleLength;
            EmitCurrentText();
        }

        public void SkipToEnd()
        {
            if (!IsRunning)
            {
                Log.Debug("Typewriter.SkipToEnd() called while not running.", null, LogCategory);
                return;
            }

            _currentVisibleLength = _fullText.Length;
            EmitCurrentText();

            Finish();
        }

        public void Cancel()
        {
            if (!IsRunning)
            {
                Log.Debug("Typewriter.Cancel() called while not running.", null, LogCategory);
                return;
            }

            IsRunning = false;
            _fullText = string.Empty;
            _buffer.Clear();
            _accumulator = 0.0f;
            _currentVisibleLength = 0;
            _onTextChanged = null;
            _onCompleted = null;
            _completedCallbackInvoked = false;
        }

        private void EmitCurrentText()
        {
            if (_onTextChanged == null)
            {
                return;
            }

            _buffer.Clear();

            if (_currentVisibleLength > 0 && _currentVisibleLength <= _fullText.Length)
            {
                _buffer.Append(_fullText, 0, _currentVisibleLength);
            }

            _onTextChanged(_buffer.ToString());
        }

        private void Finish()
        {
            if (!IsRunning)
            {
                return;
            }

            IsRunning = false;
            _accumulator = 0.0f;

            if (!_completedCallbackInvoked && _onCompleted != null)
            {
                _completedCallbackInvoked = true;
                _onCompleted();
            }
        }

        private void CompleteImmediately()
        {
            IsRunning = false;
            _currentVisibleLength = _fullText.Length;
            EmitCurrentText();

            if (_onCompleted != null)
            {
                _completedCallbackInvoked = true;
                _onCompleted();
            }
        }
    }
}

