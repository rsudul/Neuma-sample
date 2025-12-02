using System;
using System.Collections.Generic;
using Godot;
using Neuma.Core.Logging;

namespace Neuma.Core.Feeds
{
    public partial class MultiFeedController : Node
    {
        private const string LogCategory = "Core.Feeds";

        [Export]
        public Node[] FeedNodes { get; set; } = Array.Empty<Node>();

        private readonly Dictionary<FeedId, IFeed> _feeds = new();
        private readonly List<IFeed> _allFeeds = new();

        public override void _Ready()
        {
            RegisterFeedsFromNodes();
        }

        private void RegisterFeedsFromNodes()
        {
            if (FeedNodes == null || FeedNodes.Length == 0)
            {
                Log.Warn("MultiFeedController has no FeedNodes assigned. No feeds will be registered.",
                    null, LogCategory);

                return;
            }

            foreach (var node in FeedNodes)
            {
                if (node is null)
                {
                    continue;
                }

                if (node is not IFeed feed)
                {
                    Log.Warn($"Node '{node.Name}' does not implement IFeed and will be ignored.",
                        null, LogCategory);
                    continue;
                }

                var id = feed.Id;

                if (_feeds.ContainsKey(id))
                {
                    Log.Warn($"Feed with id '{id}' is already registered. Node '{node.Name}' will be ignored.",
                        null, LogCategory);
                    continue;
                }

                _feeds.Add(id, feed);
                _allFeeds.Add(feed);

                Log.Info($"Registered feed '{id}' from node '{node.Name}'.",
                    null, LogCategory);
            }

            if (_feeds.Count == 0)
            {
                Log.Warn("No feeds registered in MultiFeedController after processing FeedNodes.",
                    null, LogCategory);
            }
            else
            {
                Log.Info($"MultiFeedController initialized with {_feeds.Count} feed(s).",
                    null, LogCategory);
            }
        }

        public bool TryGetFeed(FeedId id, out IFeed? feed)
        {
            if (_feeds.TryGetValue(id, out var found))
            {
                feed = found;
                return true;
            }

            feed = null;
            return false;
        }

        public void ShowFeed(FeedId id)
        {
            if (!TryGetFeed(id, out var feed) || feed == null)
            {
                Log.Warn($"ShowFeed: feed '{id}' not found.", null, LogCategory);
                return;
            }

            Log.Debug($"ShowFeed: showing feed '{id}'.", null, LogCategory);
            feed.ShowFeed();
        }

        public void HideFeed(FeedId id)
        {
            if (!TryGetFeed(id, out var feed) || feed == null)
            {
                Log.Warn($"HideFeed: feed '{id}' not found.", null, LogCategory);
                return;
            }

            Log.Debug($"HideFeed: hiding feed '{id}'.", null, LogCategory);
            feed.HideFeed();
        }

        public void ShowAllFeeds()
        {
            Log.Debug("ShowAllFeeds: showing all registered feeds.", null, LogCategory);

            foreach (var feed in _allFeeds)
            {
                feed.ShowFeed();
            }
        }

        public void HideAllFeeds()
        {
            Log.Debug("HideAllFeeds: hiding all registered feeds.", null, LogCategory);

            foreach (var feed in _allFeeds)
            {
                feed.HideFeed();
            }
        }
    }
}

