using System;

namespace Neuma.Core.TextFeed
{
    public sealed class TextFeedVisualStyle
    {
        public string? Prefix { get; init; }
        public string? Suffix { get; init; }
        public bool UseSpeakerLabel { get; init; }
        public bool UseMonospace { get; init; }
        public string TextColorKey { get; init; } = "TextFeed.Default";
        public string BackgroundColorKey { get; init; } = "TextFeed.Background.Default";
        public float FontSizeScale { get; init; } = 1.0f;
        public float Opacity { get; init; } = 1.0f;
        public string? IconKey { get; init; }

        public static TextFeedVisualStyle Default() => new();
    }
}

