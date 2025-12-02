namespace Neuma.Core.Feeds
{
    public interface IFeed
    {
        FeedId Id { get; }

        void ShowFeed();
        void HideFeed();
    }
}

