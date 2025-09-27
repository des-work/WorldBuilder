using Genisis.Core.Data;
using Genisis.Core.Repositories;
using Genisis.Application.Services;
using Genisis.Application.Handlers;
using Genisis.Presentation.Wpf.ViewModels;
using Genisis.Presentation.Wpf.Services;
using Genisis.Infrastructure.Data;
using Genisis.Infrastructure.Repositories;
using Genisis.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using Serilog;
using System;
using System.Windows;

namespace Genisis.Presentation.Wpf;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        // Configure Serilog for logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug() // Writes to Visual Studio's Debug output window
            .WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                       "WorldBuilderAI", "Logs", "log-.txt"),
                          rollingInterval: RollingInterval.Day) // Creates a new log file each day
            .CreateLogger();

        _host = Host.CreateDefaultBuilder()
            .UseSerilog() // Use Serilog for all logging
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services);
            })
            .Build();

        // Global exception handler
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // The path for our local database file
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WorldBuilderAI", "worldbuilder.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        // Register the DbContext for Dependency Injection
        services.AddDbContext<GenesisDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Register the Repository
        services.AddScoped<IUniverseRepository, UniverseRepository>();
        services.AddScoped<IStoryRepository, StoryRepository>();
        services.AddScoped<ICharacterRepository, CharacterRepository>();
        services.AddScoped<IChapterRepository, ChapterRepository>();

        // Register AI Services
        services.AddSingleton<IAiService, OllamaAiService>();
        services.AddScoped<IPromptGenerationService, PromptGenerationService>();
        services.AddScoped<IItemHandlerFactory, ItemHandlerFactory>();

        // Register the Data Seeder
        services.AddTransient<DataSeeder>();

        // Register ViewModels
        services.AddSingleton<AIViewModel>();
        services.AddSingleton<MainViewModel>();

        // Register the MainWindow
        services.AddSingleton<MainWindow>();

        // Register UI Services
        services.AddTransient<IDialogService, DialogService>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            await _host.StartAsync();

            // Use a scope to resolve services, which is best practice
            using var scope = _host.Services.CreateScope();
            var services = scope.ServiceProvider;

            // Get the DbContext instance and migrate the database (fast operation)
            var dbContext = services.GetRequiredService<GenesisDbContext>();
            await dbContext.Database.MigrateAsync().ConfigureAwait(false);

            // Seed the database with initial data if it's empty (can be done lazily)
            var seeder = services.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync().ConfigureAwait(false);

            // Get the main view model and show the UI immediately
            var mainViewModel = services.GetRequiredService<MainViewModel>();
            var mainWindow = services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = mainViewModel; // Set the DataContext

            // Show the window immediately for faster perceived startup
            mainWindow.Show();

            // Load initial data and AI models asynchronously in the background
            _ = Task.Run(async () =>
            {
                try
                {
                    // Load AI models first (user might want to use AI features immediately)
                    await mainViewModel.AiViewModel.LoadModelsCommand.ExecuteAsync(null);

                    // Then load the universe data
                    await mainViewModel.LoadInitialDataAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error during background initialization");
                    // Don't show error to user - app is already running
                }
            });

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start correctly.");
            MessageBox.Show("A critical error occurred and the application must close. See logs for details.", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "An unhandled exception occurred.");
        MessageBox.Show("An unexpected error occurred. Please check the logs for more information.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true; // Prevents the application from crashing immediately
    }
}
