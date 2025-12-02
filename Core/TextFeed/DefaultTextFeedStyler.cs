using System;
using Neuma.Core.Logging;

namespace Neuma.Core.TextFeed
{
    public sealed class DefaultTextFeedStyler : ITextFeedStyler
    {
        private const string LogCategory = "Core.TextFeed.Styler";

        public string PrepareText(TextFeedMessage message)
        {
            if (message == null)
            {
                Log.Error("DefaultTextFeedStyler.PrepareText() called with null message.",
                    null, LogCategory);
                throw new ArgumentNullException(nameof(message));
            }

            var style = ResolveStyle(message);

            string text = message.Content;

            if (!string.IsNullOrEmpty(style.Prefix))
            {
                text = style.Prefix + text;
            }

            if (!string.IsNullOrEmpty(style.Suffix))
            {
                text = text + style.Suffix;
            }

            return text;
        }

        public TextFeedVisualStyle ResolveStyle(TextFeedMessage message)
        {
            return message.Mode switch
            {
                TextFeedMode.Dialogue => CreateDialogueStyle(message),
                TextFeedMode.SystemMessage => CreateSystemStyle(message),
                TextFeedMode.Caption => CreateCaptionStyle(message),
                TextFeedMode.Notification => CreateNotificationStyle(message),
                _ => TextFeedVisualStyle.Default()
            };
        }

        private TextFeedVisualStyle CreateDialogueStyle(TextFeedMessage msg)
        {
            string colorKey;
            switch (msg.StyleKey)
            {
                case TextFeedStyleKey.DialogueOfficer:
                    colorKey = "TextFeed.Dialogue.Officer";
                    break;

                case TextFeedStyleKey.DialogueWitness:
                    colorKey = "TextFeed.Dialogue.Witness";
                    break;

                case TextFeedStyleKey.DialogueSuspect:
                    colorKey = "TextFeed.Dialogue.Suspect";
                    break;

                default:
                    Log.Warn($"Unknown TextFeedStyleKey '{msg.StyleKey}' for Dialogue mode. Falling back to default color.",
                        null, LogCategory);
                    colorKey = "TextFeed.Dialogue.Default";
                    break;
            }

            return new TextFeedVisualStyle
            {
                UseSpeakerLabel = !string.IsNullOrWhiteSpace(msg.SpeakerName),
                FontSizeScale = 1.0f,
                TextColorKey = colorKey,
                BackgroundColorKey = "TextFeed.Background.Dialogue"
            };
        }

        private TextFeedVisualStyle CreateSystemStyle(TextFeedMessage msg)
        {
            string colorKey;
            switch (msg.StyleKey)
            {
                case TextFeedStyleKey.SystemInfo:
                    colorKey = "TextFeed.System.Info";
                    break;

                case TextFeedStyleKey.SystemWarning:
                    colorKey = "TextFeed.System.Warning";
                    break;

                case TextFeedStyleKey.SystemCritical:
                    colorKey = "TextFeed.System.Critical";
                    break;

                default:
                    Log.Warn($"Unknown TextFeedStyleKey '{msg.StyleKey}' for System mode. Falling back to default color.",
                        null, LogCategory);
                    colorKey = "TextFeed.System.Default";
                    break;
            }

            return new TextFeedVisualStyle
            {
                UseMonospace = true,
                TextColorKey = colorKey,
                BackgroundColorKey = "TextFeed.Background.System",
                Prefix = $"[{GenerateSystemCode(msg)}]"
            };
        }

        private TextFeedVisualStyle CreateCaptionStyle(TextFeedMessage msg)
        {
            return new TextFeedVisualStyle
            {
                TextColorKey = "TextFeed.Caption.Default",
                FontSizeScale = 0.9f,
                Prefix = "[",
                Suffix = "]"
            };
        }

        private TextFeedVisualStyle CreateNotificationStyle(TextFeedMessage msg)
        {
            return new TextFeedVisualStyle
            {
                TextColorKey = "TextFeed.Notification.Default",
                BackgroundColorKey = "TextFeed.Background.Notification",
                FontSizeScale = 0.85f
            };
        }

        private string GenerateSystemCode(TextFeedMessage msg)
        {
            int hash = msg.Id.GetHashCode();
            hash = Math.Abs(hash);
            int a = (hash % 9000) + 1000;
            int b = (hash / 10000) % 10;
            return $"SYS{a}/{b}";
        }
    }
}

