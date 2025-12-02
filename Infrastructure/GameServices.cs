using System;
using Microsoft.Extensions.DependencyInjection;
using Godot;

namespace Neuma.Infrastructure
{
    /// <summary>
    /// Static service locator for DI container.
    /// Initialized by Bootstrap. Do NOT create a DI container here!
    /// </summary>
    public static class GameServices
    {
        private static IServiceProvider _serviceProvider;
        private static bool _isInitialized = false;

        public static bool IsInitialized => _isInitialized;

        public static void Initialize(IServiceProvider serviceProvider)
        {
            if (_isInitialized)
            {
                return;
            }

            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            _isInitialized = true;
        }

        public static T GetService<T>() where T : class
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("GameServices not initialized. Call Initialize() first.");
            }

            return _serviceProvider.GetRequiredService<T>();
        }

        public static void Shutdown()
        {
            if (_isInitialized)
            {
                _serviceProvider = null;
                _isInitialized = false;
                GD.Print("[GameServices] Shutdown complete");
            }
        }
    }
}

