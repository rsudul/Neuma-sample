namespace Neuma.Core.TextFeed
{
    /// <summary>
    /// Interface which allows to control the flow of text feed from the level of input logic,
    /// without dependency on UI.
    /// </summary>
    public interface ITextFeedController
    {
        bool IsTyping { get; }
        void SkipOrAdvance();
    }
}

