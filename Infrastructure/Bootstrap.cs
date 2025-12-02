using Microsoft.Extensions.DependencyInjection;
using Godot;
using Neuma.Core.Input;
using Neuma.Core.Logging;

namespace Neuma.Infrastructure
{
    public partial class Bootstrap : Node
    {
        private ServiceProvider _provider = default;

        public override void _Ready()
        {
            var services = new ServiceCollection();

            services.AddGameLogging(o =>
            {
#if DEBUG
                o.GlobalMinLevel = LogLevel.Debug;
                o.ConsoleMinLevel = LogLevel.Debug;
                o.FileMinLevel = LogLevel.Info;
#else
                o.GlobalMinLevel = LogLevel.Info;
                o.ConsoleMinLevel = LogLevel.Warn;
                o.FileMinLevel = LogLevel.Info;
#endif
                o.FileDirectory = "user://logs";
                o.FilePrefix = "game";
                o.FileMaxBytes = 5 * 1024 * 1024;
                o.FileMaxFiles = 10;
            });
            services.AddRelationSystems();
            services.AddCaseProgress();
            services.AddEvidenceSystem();
            services.AddTextFeed();
            services.AddTerminalFeed();
            services.AddInputSystem();

            _provider = services.BuildServiceProvider(true);

            Log.Init(_provider.GetRequiredService<ILogger>());
            GameServices.Initialize(_provider);

            var router = _provider.GetRequiredService<InputEventRouter>();
            var gameplayHandler = _provider.GetRequiredService<GameplayInputHandler>();

            AddChild(router);
            router.RegisterHandler(InputContext.Gameplay, gameplayHandler);
            router.SetActiveContext(InputContext.Gameplay);

            Log.Info("Bootstrap initialized");

            var gs = new GameSettingsService();
            gs.Load();
            Settings.Init(gs);

            TreeExiting += OnTreeExiting;
        }

        private void OnTreeExiting()
        {
            try
            {
                Log.Flush();
            }
            catch { }

            try
            {
                _provider?.Dispose();
            }
            catch { }
        }
    }
}

