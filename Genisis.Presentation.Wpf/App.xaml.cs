using Genisis.Presentation.Wpf.Services;
using Genisis.Presentation.Wpf.Themes;
using Genisis.Presentation.Wpf.ViewModels;
using Genisis.Presentation.Wpf.Composition;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Genisis.Presentation.Wpf;

/// <summary>
/// Optimized application startup with intelligent bootscreen integration
/// </summary>
public partial class App : System.Windows.Application
{
    private readonly IHost _host;
    private IStartupService? _startupService;

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
                // Unified service registration
                services.AddWorldBuilderAppServices(context.Configuration);
            })
            .Build();

        // Global exception handler
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    // Service registration moved to Composition/ServiceRegistration.AddWorldBuilderAppServices

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            Log.Information("Starting optimized application startup");

            // Create startup service
            using var scope = _host.Services.CreateScope();
            _startupService = scope.ServiceProvider.GetRequiredService<IStartupService>();

            // Subscribe to startup events
            _startupService.ProgressChanged += OnStartupProgressChanged;
            _startupService.PhaseChanged += OnStartupPhaseChanged;
            _startupService.StartupCompleted += OnStartupCompleted;
            _startupService.StartupFailed += OnStartupFailed;

            // Start the intelligent startup process
            await _startupService.StartAsync();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start correctly");
            MessageBox.Show("A critical error occurred and the application must close. See logs for details.", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    /// <summary>
    /// Handle startup progress changes
    /// </summary>
    private void OnStartupProgressChanged(object? sender, StartupProgressEventArgs e)
    {
        Log.Debug("Startup progress: {Progress:P0} - {Phase} ({Elapsed}ms)", e.Progress, e.Phase, e.Elapsed.TotalMilliseconds);
    }

    /// <summary>
    /// Handle startup phase changes
    /// </summary>
    private void OnStartupPhaseChanged(object? sender, StartupPhaseEventArgs e)
    {
        Log.Information("Startup phase changed: {PreviousPhase} -> {CurrentPhase} (took {Duration}ms)", 
            e.PreviousPhase, e.CurrentPhase, e.PhaseDuration.TotalMilliseconds);
    }

    /// <summary>
    /// Handle startup completion
    /// </summary>
    private void OnStartupCompleted(object? sender, StartupCompletedEventArgs e)
    {
        Log.Information("Application startup completed successfully in {Duration}ms", e.TotalDuration.TotalMilliseconds);
        Log.Information("Startup metrics: {Metrics}", e.Metrics);

        // Unsubscribe from events
        if (_startupService != null)
        {
            _startupService.ProgressChanged -= OnStartupProgressChanged;
            _startupService.PhaseChanged -= OnStartupPhaseChanged;
            _startupService.StartupCompleted -= OnStartupCompleted;
            _startupService.StartupFailed -= OnStartupFailed;
        }
    }

    /// <summary>
    /// Handle startup failure
    /// </summary>
    private void OnStartupFailed(object? sender, StartupFailedEventArgs e)
    {
        Log.Error(e.Exception, "Application startup failed in phase '{Phase}' after {Elapsed}ms", e.Phase, e.Elapsed.TotalMilliseconds);
        
        // Show error to user
        MessageBox.Show($"Application startup failed: {e.Exception.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
        
        // Shutdown application
        Shutdown(1);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "An unhandled exception occurred.");
        MessageBox.Show("An unexpected error occurred. Please check the logs for more information.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true; // Prevents the application from crashing immediately
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Application shutting down");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
