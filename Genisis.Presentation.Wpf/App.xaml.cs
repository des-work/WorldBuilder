using Genisis.Core.Repositories;
using Genisis.Core.Services;
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
using System.Threading.Tasks;
using System.Windows;

namespace Genisis.Presentation.Wpf;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;

    public App()
    {
        // Configure Serilog for logging with async sinks
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug() // Writes to Visual Studio's Debug output window (synchronous but fast)
            .WriteTo.Async(a => a.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                       "WorldBuilderAI", "Logs", "log-.txt"),
                          rollingInterval: RollingInterval.Day)) // Async file sink to prevent blocking
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
        var startupTimer = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            Log.Information("Starting application startup");

            var hostTimer = System.Diagnostics.Stopwatch.StartNew();
            await _host.StartAsync();
            hostTimer.Stop();
            Log.Information("Host startup took {Elapsed}ms", hostTimer.ElapsedMilliseconds);

            // Use a scope to resolve services, which is best practice
            using var scope = _host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var serviceTimer = System.Diagnostics.Stopwatch.StartNew();

            // Get the main view model and show the UI immediately
            var mainViewModel = services.GetRequiredService<MainViewModel>();
            var mainWindow = services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = mainViewModel; // Set the DataContext

            serviceTimer.Stop();
            Log.Information("Service resolution took {Elapsed}ms", serviceTimer.ElapsedMilliseconds);

            var windowTimer = System.Diagnostics.Stopwatch.StartNew();
            // Show the window immediately for faster perceived startup
            mainWindow.Show();
            windowTimer.Stop();
            Log.Information("Window show took {Elapsed}ms", windowTimer.ElapsedMilliseconds);

            // Load everything else asynchronously in the background
            _ = Task.Run(async () =>
            {
                try
                {
                    var dataTimer = System.Diagnostics.Stopwatch.StartNew();
                    // Load universe data first (critical for basic functionality)
                    await mainWindow.Dispatcher.InvokeAsync(async () =>
                    {
                        await mainViewModel.LoadInitialDataAsync();
                    });
                    dataTimer.Stop();
                    Log.Information("Initial data loading took {Elapsed}ms", dataTimer.ElapsedMilliseconds);

                    // Database migration and seeding in background (optional)
                    try
                    {
                        var dbTimer = System.Diagnostics.Stopwatch.StartNew();
                        var dbContext = services.GetRequiredService<GenesisDbContext>();
                        await dbContext.Database.MigrateAsync().ConfigureAwait(false);

                        var seeder = services.GetRequiredService<DataSeeder>();
                        await seeder.SeedAsync().ConfigureAwait(false);
                        dbTimer.Stop();
                        Log.Information("Database migration and seeding took {Elapsed}ms", dbTimer.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Database migration/seeding failed but app continues");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error during background initialization");
                    // Don't show error to user - app is already running
                }
            });

            startupTimer.Stop();
            Log.Information("Total startup took {Elapsed}ms", startupTimer.ElapsedMilliseconds);

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            startupTimer.Stop();
            Log.Fatal(ex, "Application failed to start correctly after {Elapsed}ms", startupTimer.ElapsedMilliseconds);
            MessageBox.Show("A critical error occurred and the application must close. See logs for details.", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static async Task LoadModelsAsync(ViewModels.AIViewModel aiViewModel)
    {
        // Load models using the existing LoadModelsAsync method
        await aiViewModel.LoadModelsAsync();
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "An unhandled exception occurred.");
        MessageBox.Show("An unexpected error occurred. Please check the logs for more information.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true; // Prevents the application from crashing immediately
    }
}
