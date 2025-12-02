using System;

namespace Neuma.Core.TextFeed
{
    public interface ITypewriter
    {
        void Start(string fullText, float charsPerSecond, Action<string> onTextChanged, Action onCompleted);
        void Update(float deltaSeconds);
        void SkipToEnd();
        void Cancel();
        bool IsRunning { get; }
        float Progress { get; }
    }
}

