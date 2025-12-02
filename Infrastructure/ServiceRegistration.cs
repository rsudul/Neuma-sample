using System;
using Microsoft.Extensions.DependencyInjection;
using Neuma.Core.Cases;
using Neuma.Core.EvidenceSystem;
using Neuma.Core.Logging;
using Neuma.Core.Relations;
using Neuma.Core.TextFeed;
using Neuma.Infrastructure.Relations;
using Neuma.Core.TerminalFeed;
using Neuma.Infrastructure.TextFeed;
using Neuma.Core.Input;

namespace Neuma.Infrastructure
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddGameLogging(this IServiceCollection services, Action<LoggerOptions>? configure = null)
        {
            var options = new LoggerOptions();
            configure?.Invoke(options);

            services.AddSingleton(options);
            services.AddSingleton<ILogSink>(sp => new ConsoleSink(minLevel: options.ConsoleMinLevel));
            services.AddSingleton<ILogSink>(sp => new FileSink(
                minLevel: options.FileMinLevel,
                maxBytes: options.FileMaxBytes,
                maxFiles: options.FileMaxFiles,
                directory: options.FileDirectory,
                filePrefix: options.FilePrefix
            ));
            services.AddSingleton<ILogger, Logger>();

            return services;
        }

        public static IServiceCollection AddTextFeed(this IServiceCollection services)
        {
            services.AddSingleton<ITextFeedService, TextFeedService>();
            services.AddSingleton<ITextFeedStyler, DefaultTextFeedStyler>();
            services.AddSingleton<TextFeedControllerProxy>();
            services.AddSingleton<ITextFeedController>(sp => sp.GetRequiredService<TextFeedControllerProxy>());
            return services;
        }

        public static IServiceCollection AddInputSystem(this IServiceCollection services)
        {
            services.AddSingleton<IInputService, InputService>();
            services.AddSingleton<InputEventRouter>();
            services.AddSingleton<GameplayInputHandler>();
            return services;
        }

        public static IServiceCollection AddLinkAnchorProviders(this IServiceCollection services)
        {
            services.AddSingleton<ILinkAnchorProvider, EvidenceAnchorProvider>();
            services.AddSingleton<ILinkAnchorProvider, TranscriptAnchorProvider>();

            return services;
        }

        public static IServiceCollection AddRelationSystems(this IServiceCollection services)
        {
            services.AddLinkAnchorProviders();

            services.AddSingleton<IPairMatchService, PairMatchRelationSystem>();
            services.AddSingleton<IFoundRelationsTracker, FoundRelationsTracker>();
            services.AddSingleton<LinkSelectionController>();
            services.AddSingleton<IRelationDefinitionRepository, JsonRelationDefinitionRepository>();

            return services;
        }

        public static IServiceCollection AddCaseProgress(this IServiceCollection services)
        {
            services.AddSingleton<ICaseProgressTracker, CaseProgressTracker>();
            return services;
        }

        public static IServiceCollection AddEvidenceSystem(this IServiceCollection services)
        {
            services.AddSingleton<IEvidenceUnlockService, EvidenceUnlockService>();
            return services;
        }

        public static IServiceCollection AddTerminalFeed(this IServiceCollection services)
        {
            services.AddSingleton<ITerminalController, TerminalController>();
            return services;
        }
    }
}

