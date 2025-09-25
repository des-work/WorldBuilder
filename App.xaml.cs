using Genisis.Core.Data;
using Genisis.Core.Repositories;
using Genisis.App.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using Serilog;
using System;
using System.Windows;

namespace Genisis.App;

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
                                       "GenisisAI", "Logs", "log-.txt"), 
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
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GenisisAI", "genisis.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        // Register the DbContext for Dependency Injection
        services.AddDbContext<GenesisDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Register the Repository
        services.AddScoped<IUniverseRepository, UniverseRepository>();
        services.AddScoped<IStoryRepository, StoryRepository>();
        services.AddScoped<ICharacterRepository, CharacterRepository>();
        services.AddScoped<IChapterRepository, ChapterRepository>();

        // Register the Data Seeder
        services.AddTransient<DataSeeder>();

        // Register the ViewModel
        services.AddSingleton<MainViewModel>();

        // Register the MainWindow
        services.AddSingleton<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            await _host.StartAsync();

            // Use a scope to resolve services, which is best practice
            using var scope = _host.Services.CreateScope();
            var services = scope.ServiceProvider;

            // Get the DbContext instance and ensure the database is created
            var dbContext = services.GetRequiredService<GenesisDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            // Seed the database with initial data if it's empty
            var seeder = services.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync();

            // Load the main view model and its data
            var mainViewModel = services.GetRequiredService<MainViewModel>();
            await mainViewModel.LoadInitialDataAsync();

            var mainWindow = services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = mainViewModel; // Set the DataContext
            mainWindow.Show();

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