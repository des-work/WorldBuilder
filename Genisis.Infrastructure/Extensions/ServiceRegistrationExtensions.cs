using Genisis.Core.Configuration;
using Genisis.Core.Repositories;
using Genisis.Core.Services;
using Genisis.Infrastructure.Data;
using Genisis.Infrastructure.Repositories;
using Genisis.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genisis.Core.Extensions;

/// <summary>
/// Extension methods for registering WorldBuilder services for tests and host composition.
/// Located in Infrastructure to access DbContext/Repositories while preserving namespace.
/// </summary>
public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddWorldBuilderServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind typed configuration
        services.Configure<WorldBuilderConfiguration>(configuration);

        // Database (tests can override with in-memory via AddDbContext after calling this)
        services.AddDbContext<GenesisDbContext>((sp, options) =>
        {
            var cfg = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<WorldBuilderConfiguration>>().Value;
            var dbPath = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                dbPath = System.IO.Path.Combine(appData, "WorldBuilderAI", "worldbuilder.db");
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dbPath)!);
            }

            options.UseSqlite($"Data Source={dbPath}", sqliteOptions =>
            {
                sqliteOptions.CommandTimeout(cfg.Database.CommandTimeout);
                sqliteOptions.MigrationsAssembly("Genisis.Infrastructure");
            });

            options.EnableSensitiveDataLogging(cfg.Database.EnableSensitiveDataLogging);
            options.EnableDetailedErrors(cfg.Database.EnableDetailedErrors);
        });

        // Caching
        services.AddMemoryCache();

        // Repositories
        services.AddScoped<IUniverseRepository, UniverseRepository>();
        services.AddScoped<IStoryRepository, StoryRepository>();
        services.AddScoped<ICharacterRepository, CharacterRepository>();
        services.AddScoped<IChapterRepository, ChapterRepository>();

        // Domain services
        services.AddScoped<IUniverseDomainService, UniverseDomainService>();

        // AI service (local Ollama) via typed HttpClient
        services.AddHttpClient<IAiService, OllamaAiService>(client =>
        {
            var baseUrl = configuration["AI:OllamaBaseUrl"] ?? "http://localhost:11434";
            var timeoutSeconds = int.TryParse(configuration["AI:RequestTimeout"], out var t) ? t : 300;
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        });

        // Data seeding
        services.AddTransient<DataSeeder>();

        return services;
    }

    /// <summary>
    /// Register minimal mock services used by tests. These are simple stubs that avoid external dependencies.
    /// </summary>
    public static IServiceCollection AddMockServices(this IServiceCollection services)
    {
        services.AddSingleton<IAiService, MockAiService>();
        services.AddSingleton<ILocalModelManager, MockLocalModelManager>();
        services.AddSingleton<IInitialSetupService, MockInitialSetupService>();
        services.AddSingleton<ISmartQuerySystem, MockSmartQuerySystem>();
        return services;
    }

    /// <summary>
    /// Allow tests to inject typed config
    /// </summary>
    public static IServiceCollection AddTestConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WorldBuilderConfiguration>(configuration);
        return services;
    }

    // Lightweight test doubles
    private sealed class MockAiService : IAiService
    {
        public Task<List<string>> GetLocalModelsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new List<string> { "tinyllama", "llama3" });

        public IAsyncEnumerable<string> StreamCompletionAsync(string model, string prompt, CancellationToken cancellationToken = default)
        {
            async IAsyncEnumerable<string> Impl()
            {
                yield return "Mock response chunk.";
                await Task.CompletedTask;
            }
            return Impl();
        }

        public void ClearModelCache() { }
    }

    private sealed class MockLocalModelManager : ILocalModelManager
    {
        public Task<bool> ValidateModelAsync(string modelName, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<IReadOnlyList<string>> DiscoverModelsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult((IReadOnlyList<string>)new List<string> { "tinyllama", "llama3" });
    }

    private sealed class MockInitialSetupService : IInitialSetupService
    {
        public Task<bool> EnsureInitializedAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(true);
    }

    private sealed class MockSmartQuerySystem : ISmartQuerySystem
    {
        public Task<string> ProcessQueryAsync(string query, CancellationToken cancellationToken = default)
            => Task.FromResult("Processed: " + query);
    }
}
