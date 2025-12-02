using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Neuma.Infrastructure
{
    /// <summary>
    /// Manages persistent game settings including tutorial hints.
    /// Settings are saved to user:// directory in Godot.
    /// </summary>
    public sealed class GameSettingsService
    {
        private readonly string _path;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public GameSettings Current { get; private set; } = new();

        public string PathToFile => _path;

        public event EventHandler? OnChanged;

        public GameSettingsService(string? path = null)
        {
            var root = ResolveUserRoot();
            _path = path ?? Path.Combine(root, "settings.json");
        }

        private static string ResolveUserRoot()
        {
            try
            {
                var godotAsm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name?.StartsWith("Godot", StringComparison.OrdinalIgnoreCase) == true);

                if (godotAsm != null)
                {
                    var psType = godotAsm.GetType("Godot.ProjectSettings");
                    var method = psType?.GetMethod("GlobalizePath", new[] { typeof(string) });
                    var user = method?.Invoke(null, new object[] { "user://" }) as string;
                    if (!string.IsNullOrEmpty(user))
                    {
                        return user!;
                    }
                }
            }
            catch { }

            var temp = Path.Combine(Path.GetTempPath(), "app_userdata");
            try
            {
                Directory.CreateDirectory(temp);
            }
            catch { }

            return temp;
        }

        public void Load()
        {
            try
            {
                if (File.Exists(_path))
                {
                    var json = File.ReadAllText(_path, Encoding.UTF8);
                    var loaded = JsonSerializer.Deserialize<GameSettings>(json, JsonOptions);
                    if (loaded != null)
                    {
                        Current = loaded;
                    }
                }

                ClampVolumes();
            }
            catch { }

            OnChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Save()
        {
            try
            {
                ClampVolumes();

                var dir = System.IO.Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var json = JsonSerializer.Serialize(Current, JsonOptions);
                var tmp = _path + ".tmp";

                File.WriteAllText(tmp, json, new UTF8Encoding(false));
                File.Copy(tmp, _path, true);
                File.Delete(tmp);
            }
            catch { }
        }

        public void Update(Action<GameSettings> mutate)
        {
            if (mutate == null)
            {
                return;
            }

            try
            {
                mutate(Current);
                Save();
            }
            catch { }

            OnChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ResetToDefaults()
        {
            Current = new GameSettings();
            Save();
            OnChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ClampVolumes()
        {
            Current.MasterVolume = Clamp01(Current.MasterVolume);
            Current.MusicVolume = Clamp01(Current.MusicVolume);
            Current.SfxVolume = Clamp01(Current.SfxVolume);
        }

        private static float Clamp01(float v)
        {
            if (v < 0.0f)
            {
                return 0.0f;
            }

            if (v > 1.0f)
            {
                return 1.0f;
            }

            return v;
        }
    }
}
