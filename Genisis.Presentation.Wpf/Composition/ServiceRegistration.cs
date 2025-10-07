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
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
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
        services.AddDbContext<GenesisDbContext>((sp, options) =>
        {
            var cfg = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<WorldBuilderConfiguration>>().Value;
            var conn = configuration.GetConnectionString("DefaultConnection");
            var dbPath = string.IsNullOrWhiteSpace(conn)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WorldBuilderAI", "worldbuilder.db")
                : conn;
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

            options.UseSqlite($"Data Source={dbPath}", sqliteOptions =>
            {
                sqliteOptions.CommandTimeout(cfg.Database.CommandTimeout);
                sqliteOptions.MigrationsAssembly("Genisis.Infrastructure");
            });

            options.EnableSensitiveDataLogging(cfg.Database.EnableSensitiveDataLogging);
            options.EnableDetailedErrors(cfg.Database.EnableDetailedErrors);
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

        // AI services (local Ollama integration) via typed HttpClient
        services.AddHttpClient<IAiService, OllamaAiService>(client =>
        {
            var baseUrl = configuration["AI:OllamaBaseUrl"] ?? "http://localhost:11434";
            var timeoutSeconds = int.TryParse(configuration["AI:RequestTimeout"], out var t) ? t : 300;
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        });

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
