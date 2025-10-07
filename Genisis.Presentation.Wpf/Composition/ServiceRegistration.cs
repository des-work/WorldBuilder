using System;
using System.IO;
using Genisis.Application.Services;
using Genisis.Core.Configuration;
using Genisis.Core.Repositories;
using Genisis.Infrastructure.Data;
using Genisis.Infrastructure.Repositories;
using Genisis.Infrastructure.Services;
using Genisis.Presentation.Wpf.Services;
using Genisis.Presentation.Wpf.Themes;
using Genisis.Presentation.Wpf.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Genisis.Presentation.Wpf.Composition;

/// <summary>
/// Composition root for WPF application service registration.
/// Consolidates DI setup into a single, reusable extension.
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddWorldBuilderAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind typed configuration for future use across app
        services.Configure<WorldBuilderConfiguration>(configuration);

        // Database (SQLite in AppData by default, overridable via connection string)
        services.AddDbContext<GenesisDbContext>(options =>
        {
            var dbPath = configuration.GetConnectionString("DefaultConnection")
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WorldBuilderAI", "worldbuilder.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

            options.UseSqlite($"Data Source={dbPath}", sqliteOptions =>
            {
                sqliteOptions.CommandTimeout(30);
                sqliteOptions.MigrationsAssembly("Genisis.Infrastructure");
            });

            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });

        // Repositories
        services.AddScoped<IUniverseRepository, UniverseRepository>();
        services.AddScoped<IStoryRepository, StoryRepository>();
        services.AddScoped<ICharacterRepository, CharacterRepository>();
        services.AddScoped<IChapterRepository, ChapterRepository>();

        // Domain/Application services
        services.AddScoped<IPromptGenerationService, PromptGenerationService>();
        services.AddScoped<IItemHandlerFactory, ItemHandlerFactory>();
        services.AddScoped<IUniverseDomainService, UniverseDomainService>();

        // AI services (local Ollama integration)
        services.AddSingleton<IAiService, OllamaAiService>();

        // Caching and data seeding
        services.AddMemoryCache();
        services.AddTransient<DataSeeder>();

        // ViewModels used by the main window and AI panel
        services.AddSingleton<AIViewModel>();
        services.AddSingleton<MainViewModel>();

        // Views
        services.AddSingleton<MainWindowV3>();

        // UI services
        services.AddTransient<IDialogService, DialogService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IStartupService, StartupService>();

        return services;
    }
}

