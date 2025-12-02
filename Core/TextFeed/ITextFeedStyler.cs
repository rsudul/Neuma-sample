namespace Neuma.Core.TextFeed
{
    public interface ITextFeedStyler
    {
        string PrepareText(TextFeedMessage message);
        TextFeedVisualStyle ResolveStyle(TextFeedMessage message);
    }
}

