using Genisis.Application.Universe.Commands;
using Genisis.Application.Universe.Commands.Validators;
using Genisis.Core.Services;
using Genisis.Infrastructure.Data;
using Genisis.Infrastructure.Repositories;
using Genisis.Infrastructure.Services;
using Genisis.Presentation.Wpf.Services;
using Genisis.Presentation.Wpf.Themes;
using Genisis.Presentation.Wpf.ViewModels;
using Genisis.Presentation.Wpf.Views;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Reflection;

namespace Genisis.Core.Extensions;

/// <summary>
/// Extension methods for service registration
/// </summary>
public static class ServiceRegistrationExtensions
{
    /// <summary>
    /// Add all WorldBuilder services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddWorldBuilderServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Core Infrastructure
        services.AddWorldBuilderInfrastructure(configuration);
        
        // Application Layer
        services.AddWorldBuilderApplication();
        
        // AI Services
        services.AddWorldBuilderAI(configuration);
        
        // Presentation Layer
        services.AddWorldBuilderPresentation();
        
        // Testing Infrastructure
        services.AddWorldBuilderTesting();
        
        return services;
    }
    
    /// <summary>
    /// Add WorldBuilder infrastructure services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    private static IServiceCollection AddWorldBuilderInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
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
        
        // Domain Services
        services.AddScoped<IUniverseDomainService, UniverseDomainService>();
        
        // Caching
        services.AddMemoryCache();
        
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddSerilog();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        return services;
    }
    
    /// <summary>
    /// Add WorldBuilder application services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    private static IServiceCollection AddWorldBuilderApplication(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateUniverseCommand).Assembly);
        });
        
        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(CreateUniverseCommandValidator).Assembly);
        
        // Application Services
        services.AddScoped<IPromptGenerationService, PromptGenerationService>();
        services.AddScoped<IItemHandlerFactory, ItemHandlerFactory>();
        
        return services;
    }
    
    /// <summary>
    /// Add WorldBuilder AI services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    private static IServiceCollection AddWorldBuilderAI(this IServiceCollection services, IConfiguration configuration)
    {
        // AI Services
        services.AddSingleton<IAiService, OllamaAiService>();
        services.AddSingleton<ILocalModelManager, LocalModelManager>();
        services.AddSingleton<IInitialSetupService, InitialSetupService>();
        services.AddSingleton<ISmartQuerySystem, SmartQuerySystem>();
        services.AddSingleton<IDynamicPromptGenerator, DynamicPromptGenerator>();
        services.AddSingleton<IStoryContextDatabase, StoryContextDatabase>();
        services.AddSingleton<IContextAwareAIService, ContextAwareAIService>();
        
        // Performance Services
        services.AddSingleton<IModelPerformanceProfiler, ModelPerformanceProfiler>();
        services.AddSingleton<IModelSelectionOptimizer, ModelSelectionOptimizer>();
        services.AddSingleton<IResponseCacheSystem, ResponseCacheSystem>();
        services.AddSingleton<IPerformanceMonitor, PerformanceMonitor>();
        
        // Resilience Services
        services.AddSingleton<ICircuitBreaker, CircuitBreaker>();
        services.AddSingleton<IRetryPolicy, ExponentialBackoffRetryPolicy>();
        services.AddSingleton<IBackgroundProcessingService, BackgroundProcessingService>();
        
        // HTTP Client for AI
        services.AddHttpClient<OllamaAiService>(client =>
        {
            client.BaseAddress = new Uri(configuration["AI:OllamaBaseUrl"] ?? "http://localhost:11434");
            client.Timeout = TimeSpan.FromMinutes(5);
        });
        
        return services;
    }
    
    /// <summary>
    /// Add WorldBuilder presentation services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    private static IServiceCollection AddWorldBuilderPresentation(this IServiceCollection services)
    {
        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<AIViewModel>();
        services.AddSingleton<UniverseViewModel>();
        services.AddSingleton<StoryViewModel>();
        services.AddSingleton<CharacterViewModel>();
        services.AddSingleton<ChapterViewModel>();
        services.AddSingleton<NonLinearStoryViewModel>();
        services.AddSingleton<EnhancedAIViewModel>();
        
        // Services
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IStartupService, StartupService>();
        
        // Theme Providers
        services.AddSingleton<IThemeProvider, FantasyThemeProvider>();
        services.AddSingleton<IThemeProvider, SciFiThemeProvider>();
        services.AddSingleton<IThemeProvider, ClassicThemeProvider>();
        services.AddSingleton<IThemeProvider, HorrorThemeProvider>();
        
        // Views
        services.AddSingleton<MainWindowV3>();
        services.AddSingleton<EnhancedBootscreenView>();
        
        return services;
    }
    
    /// <summary>
    /// Add WorldBuilder testing services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    private static IServiceCollection AddWorldBuilderTesting(this IServiceCollection services)
    {
        // Test-specific services
        services.AddSingleton<ITestDataBuilder, TestDataBuilder>();
        services.AddSingleton<ITestScenarioRunner, TestScenarioRunner>();
        services.AddSingleton<ITestPerformanceMonitor, TestPerformanceMonitor>();
        
        return services;
    }
    
    /// <summary>
    /// Add mock services for testing
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMockServices(this IServiceCollection services)
    {
        // Mock AI services
        services.AddSingleton<IAiService, MockAiService>();
        services.AddSingleton<ILocalModelManager, MockLocalModelManager>();
        services.AddSingleton<IInitialSetupService, MockInitialSetupService>();
        services.AddSingleton<ISmartQuerySystem, MockSmartQuerySystem>();
        
        // Mock performance services
        services.AddSingleton<IModelPerformanceProfiler, MockModelPerformanceProfiler>();
        services.AddSingleton<IPerformanceMonitor, MockPerformanceMonitor>();
        
        return services;
    }
    
    /// <summary>
    /// Add test configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddTestConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Override with test-specific settings
        services.Configure<WorldBuilderConfiguration>(configuration);
        
        return services;
    }
}
