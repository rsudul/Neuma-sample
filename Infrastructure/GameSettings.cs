using System.Text.Json.Serialization;

namespace Neuma.Infrastructure
{
    public sealed class GameSettings
    {
        public float MasterVolume { get; set; } = 1.0f;
        public float MusicVolume { get; set; } = 1.0f;
        public float SfxVolume { get; set; } = 1.0f;

        public float TextFeedTypingSpeed { get; set; } = 40.0f;

        public string Language { get; set; } = "en";

        public bool DebugMode { get; set; } = false;
        public bool TestMode { get; set; } = false;

        [JsonIgnore]
        public float EffectiveMusicVolume => MasterVolume * MusicVolume;
        [JsonIgnore]
        public float EffectiveSfxVolume => MasterVolume * SfxVolume;
    }
}
