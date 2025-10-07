using System.Diagnostics;
using System.Windows.Threading;
using Genisis.Presentation.Wpf.Themes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Service for managing application startup process with intelligent optimization
/// </summary>
public class StartupService : IStartupService
{
    private readonly IHost _host;
    private readonly Dispatcher _dispatcher;
    private readonly Stopwatch _startupTimer = new();
    private readonly StartupMetrics _metrics = new();
    private readonly List<StartupPhase> _phases = new();
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isInProgress;
    private bool _isComplete;
    private double _progress;
    private string _currentPhase = "Initializing";

    public StartupService(IHost host, Dispatcher dispatcher)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        
        InitializePhases();
    }

    public double Progress
    {
        get => _progress;
        private set
        {
            if (Math.Abs(_progress - value) > 0.01) // Only update if significant change
            {
                _progress = value;
                OnProgressChanged();
            }
        }
    }

    public string CurrentPhase
    {
        get => _currentPhase;
        private set
        {
            if (_currentPhase != value)
            {
                var previousPhase = _currentPhase;
                var phaseDuration = _startupTimer.Elapsed;
                _currentPhase = value;
                OnPhaseChanged(previousPhase, phaseDuration);
            }
        }
    }

    public bool IsComplete => _isComplete;
    public bool IsInProgress => _isInProgress;

    public event EventHandler<StartupProgressEventArgs>? ProgressChanged;
    public event EventHandler<StartupPhaseEventArgs>? PhaseChanged;
    public event EventHandler<StartupCompletedEventArgs>? StartupCompleted;
    public event EventHandler<StartupFailedEventArgs>? StartupFailed;

    public async Task StartAsync()
    {
        if (_isInProgress) return;

        _isInProgress = true;
        _isComplete = false;
        _cancellationTokenSource = new CancellationTokenSource();
        _startupTimer.Restart();

        try
        {
            Log.Information("Starting intelligent application startup");

            // Phase 1: Host Startup
            await ExecutePhaseAsync("Host Startup", async () =>
            {
                var phaseTimer = Stopwatch.StartNew();
                await _host.StartAsync();
                phaseTimer.Stop();
                _metrics.HostStartupDuration = phaseTimer.Elapsed;
                Log.Information("Host startup completed in {Elapsed}ms", phaseTimer.ElapsedMilliseconds);
            });

            // Phase 2: Service Resolution
            await ExecutePhaseAsync("Service Resolution", async () =>
            {
                var phaseTimer = Stopwatch.StartNew();
                using var scope = _host.Services.CreateScope();
                var services = scope.ServiceProvider;
                
                // Resolve critical services first
                var criticalServices = await ResolveCriticalServicesAsync(services);
                _metrics.ServicesResolved = criticalServices.Count;
                
                phaseTimer.Stop();
                _metrics.ServiceResolutionDuration = phaseTimer.Elapsed;
                Log.Information("Service resolution completed in {Elapsed}ms", phaseTimer.ElapsedMilliseconds);
            });

            // Phase 3: Theme Initialization
            await ExecutePhaseAsync("Theme Initialization", async () =>
            {
                var phaseTimer = Stopwatch.StartNew();
                await InitializeThemeSystemAsync();
                phaseTimer.Stop();
                _metrics.ThemeInitializationDuration = phaseTimer.Elapsed;
                Log.Information("Theme initialization completed in {Elapsed}ms", phaseTimer.ElapsedMilliseconds);
            });

            // Phase 4: Bootscreen Animation
            await ExecutePhaseAsync("Bootscreen Animation", async () =>
            {
                var phaseTimer = Stopwatch.StartNew();
                await StartBootscreenAsync();
                phaseTimer.Stop();
                _metrics.BootscreenDuration = phaseTimer.Elapsed;
                Log.Information("Bootscreen animation completed in {Elapsed}ms", phaseTimer.ElapsedMilliseconds);
            });

            // Phase 5: Background Initialization
            await ExecutePhaseAsync("Background Initialization", async () =>
            {
                await StartBackgroundInitializationAsync();
            });

            // Complete startup
            _startupTimer.Stop();
            _metrics.TotalDuration = _startupTimer.Elapsed;
            _metrics.MemoryUsed = GC.GetTotalMemory(false);
            
            _isComplete = true;
            _isInProgress = false;
            Progress = 1.0;

            Log.Information("Application startup completed successfully in {Elapsed}ms", _startupTimer.ElapsedMilliseconds);
            OnStartupCompleted();
        }
        catch (Exception ex)
        {
            _startupTimer.Stop();
            Log.Error(ex, "Application startup failed in phase '{Phase}' after {Elapsed}ms", CurrentPhase, _startupTimer.ElapsedMilliseconds);
            OnStartupFailed(ex);
        }
        finally
        {
            _isInProgress = false;
        }
    }

    public void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        _isInProgress = false;
    }

    public StartupMetrics GetMetrics()
    {
        return new StartupMetrics
        {
            TotalDuration = _metrics.TotalDuration,
            HostStartupDuration = _metrics.HostStartupDuration,
            ServiceResolutionDuration = _metrics.ServiceResolutionDuration,
            ThemeInitializationDuration = _metrics.ThemeInitializationDuration,
            BootscreenDuration = _metrics.BootscreenDuration,
            DataLoadingDuration = _metrics.DataLoadingDuration,
            DatabaseInitializationDuration = _metrics.DatabaseInitializationDuration,
            ServicesResolved = _metrics.ServicesResolved,
            MemoryUsed = _metrics.MemoryUsed,
            DatabaseMigrationPerformed = _metrics.DatabaseMigrationPerformed,
            DataSeedingPerformed = _metrics.DataSeedingPerformed
        };
    }

    /// <summary>
    /// Initialize startup phases
    /// </summary>
    private void InitializePhases()
    {
        _phases.AddRange(new[]
        {
            new StartupPhase("Host Startup", 0.1),
            new StartupPhase("Service Resolution", 0.2),
            new StartupPhase("Theme Initialization", 0.3),
            new StartupPhase("Bootscreen Animation", 0.4),
            new StartupPhase("Background Initialization", 0.5),
            new StartupPhase("Data Loading", 0.7),
            new StartupPhase("Database Initialization", 0.9),
            new StartupPhase("Complete", 1.0)
        });
    }

    /// <summary>
    /// Execute a startup phase
    /// </summary>
    private async Task ExecutePhaseAsync(string phaseName, Func<Task> phaseAction)
    {
        if (_cancellationTokenSource?.Token.IsCancellationRequested == true)
            return;

        CurrentPhase = phaseName;
        var phase = _phases.FirstOrDefault(p => p.Name == phaseName);
        if (phase != null)
        {
            Progress = phase.Progress;
        }

        try
        {
            await phaseAction();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Phase '{Phase}' failed", phaseName);
            throw;
        }
    }

    /// <summary>
    /// Resolve critical services
    /// </summary>
    private async Task<List<object>> ResolveCriticalServicesAsync(IServiceProvider services)
    {
        var criticalServices = new List<object>();

        // Resolve only essential services for initial startup
        try
        {
            // Main window and view model
            var mainWindow = services.GetRequiredService<MainWindowV3>();
            var mainViewModel = services.GetRequiredService<MainViewModel>();
            criticalServices.Add(mainWindow);
            criticalServices.Add(mainViewModel);

            // Theme service
            var themeService = services.GetRequiredService<IThemeService>();
            criticalServices.Add(themeService);

            // Set data context
            mainWindow.DataContext = mainViewModel;

            // Show window immediately for perceived performance
            await _dispatcher.InvokeAsync(() => mainWindow.Show());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to resolve critical services");
            throw;
        }

        return criticalServices;
    }

    /// <summary>
    /// Initialize theme system
    /// </summary>
    private async Task InitializeThemeSystemAsync()
    {
        try
        {
            using var scope = _host.Services.CreateScope();
            var themeService = scope.ServiceProvider.GetRequiredService<IThemeService>();
            await themeService.InitializeAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize theme system");
            throw;
        }
    }

    /// <summary>
    /// Start bootscreen animation
    /// </summary>
    private async Task StartBootscreenAsync()
    {
        try
        {
            using var scope = _host.Services.CreateScope();
            var themeService = scope.ServiceProvider.GetRequiredService<IThemeService>();
            var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindowV3>();

            if (themeService.CurrentTheme != null)
            {
                // Start bootscreen animation
                await _dispatcher.InvokeAsync(async () =>
                {
                    await mainWindow.StartBootscreenAsync(themeService.CurrentTheme);
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start bootscreen");
            throw;
        }
    }

    /// <summary>
    /// Start background initialization
    /// </summary>
    private async Task StartBackgroundInitializationAsync()
    {
        // Start background tasks without blocking
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _host.Services.CreateScope();
                var services = scope.ServiceProvider;

                // Load initial data
                await ExecutePhaseAsync("Data Loading", async () =>
                {
                    var phaseTimer = Stopwatch.StartNew();
                    var mainViewModel = services.GetRequiredService<MainViewModel>();
                    await mainViewModel.LoadInitialDataAsync();
                    phaseTimer.Stop();
                    _metrics.DataLoadingDuration = phaseTimer.Elapsed;
                });

                // Database initialization
                await ExecutePhaseAsync("Database Initialization", async () =>
                {
                    var phaseTimer = Stopwatch.StartNew();
                    await InitializeDatabaseAsync(services);
                    phaseTimer.Stop();
                    _metrics.DatabaseInitializationDuration = phaseTimer.Elapsed;
                });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Background initialization failed but app continues");
            }
        });
    }

    /// <summary>
    /// Initialize database
    /// </summary>
    private async Task InitializeDatabaseAsync(IServiceProvider services)
    {
        try
        {
            var dbContext = services.GetRequiredService<GenesisDbContext>();
            
            // Check if migration is needed
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync();
                _metrics.DatabaseMigrationPerformed = true;
                Log.Information("Database migration completed");
            }

            // Seed database if needed
            var seeder = services.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync();
            _metrics.DataSeedingPerformed = true;
            Log.Information("Database seeding completed");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Database initialization failed but app continues");
        }
    }

    /// <summary>
    /// Raise progress changed event
    /// </summary>
    private void OnProgressChanged()
    {
        ProgressChanged?.Invoke(this, new StartupProgressEventArgs(Progress, CurrentPhase, _startupTimer.Elapsed));
    }

    /// <summary>
    /// Raise phase changed event
    /// </summary>
    private void OnPhaseChanged(string previousPhase, TimeSpan phaseDuration)
    {
        PhaseChanged?.Invoke(this, new StartupPhaseEventArgs(previousPhase, CurrentPhase, phaseDuration));
    }

    /// <summary>
    /// Raise startup completed event
    /// </summary>
    private void OnStartupCompleted()
    {
        StartupCompleted?.Invoke(this, new StartupCompletedEventArgs(_startupTimer.Elapsed, GetMetrics()));
    }

    /// <summary>
    /// Raise startup failed event
    /// </summary>
    private void OnStartupFailed(Exception exception)
    {
        StartupFailed?.Invoke(this, new StartupFailedEventArgs(exception, CurrentPhase, _startupTimer.Elapsed));
    }
}

/// <summary>
/// Startup phase definition
/// </summary>
public class StartupPhase
{
    public string Name { get; }
    public double Progress { get; }

    public StartupPhase(string name, double progress)
    {
        Name = name;
        Progress = progress;
    }
}
