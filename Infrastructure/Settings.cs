using System;

namespace Neuma.Infrastructure
{
    /// <summary>
    /// Global access to GameSettingsService, based on facade.
    /// </summary>
    public static class Settings
    {
        private static GameSettingsService _svc = new();

        public static void Init(GameSettingsService service)
        {
            _svc = service ?? throw new ArgumentNullException(nameof(service));
        }

        public static GameSettings Current => _svc.Current;

        public static event EventHandler? OnChanged
        {
            add { _svc.OnChanged += value; }
            remove { _svc.OnChanged -= value; }
        }

        public static void Load() => _svc.Load();

        public static void Update(Action<GameSettings> mutate) => _svc.Update(mutate);

        public static void ResetToDefaults() => _svc.ResetToDefaults();
    }
}

