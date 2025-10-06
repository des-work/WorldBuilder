using Genisis.Presentation.Wpf.Themes;

namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Service for managing application startup process
/// </summary>
public interface IStartupService
{
    /// <summary>
    /// Startup progress (0-1)
    /// </summary>
    double Progress { get; }

    /// <summary>
    /// Current startup phase
    /// </summary>
    string CurrentPhase { get; }

    /// <summary>
    /// Whether startup is complete
    /// </summary>
    bool IsComplete { get; }

    /// <summary>
    /// Whether startup is in progress
    /// </summary>
    bool IsInProgress { get; }

    /// <summary>
    /// Event raised when startup progress changes
    /// </summary>
    event EventHandler<StartupProgressEventArgs>? ProgressChanged;

    /// <summary>
    /// Event raised when startup phase changes
    /// </summary>
    event EventHandler<StartupPhaseEventArgs>? PhaseChanged;

    /// <summary>
    /// Event raised when startup completes
    /// </summary>
    event EventHandler<StartupCompletedEventArgs>? StartupCompleted;

    /// <summary>
    /// Event raised when startup fails
    /// </summary>
    event EventHandler<StartupFailedEventArgs>? StartupFailed;

    /// <summary>
    /// Start the application startup process
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Cancel startup process
    /// </summary>
    void Cancel();

    /// <summary>
    /// Get startup performance metrics
    /// </summary>
    StartupMetrics GetMetrics();
}

/// <summary>
/// Startup progress event args
/// </summary>
public class StartupProgressEventArgs : EventArgs
{
    public double Progress { get; }
    public string Phase { get; }
    public TimeSpan Elapsed { get; }

    public StartupProgressEventArgs(double progress, string phase, TimeSpan elapsed)
    {
        Progress = progress;
        Phase = phase;
        Elapsed = elapsed;
    }
}

/// <summary>
/// Startup phase event args
/// </summary>
public class StartupPhaseEventArgs : EventArgs
{
    public string PreviousPhase { get; }
    public string CurrentPhase { get; }
    public TimeSpan PhaseDuration { get; }

    public StartupPhaseEventArgs(string previousPhase, string currentPhase, TimeSpan phaseDuration)
    {
        PreviousPhase = previousPhase;
        CurrentPhase = currentPhase;
        PhaseDuration = phaseDuration;
    }
}

/// <summary>
/// Startup completed event args
/// </summary>
public class StartupCompletedEventArgs : EventArgs
{
    public TimeSpan TotalDuration { get; }
    public StartupMetrics Metrics { get; }

    public StartupCompletedEventArgs(TimeSpan totalDuration, StartupMetrics metrics)
    {
        TotalDuration = totalDuration;
        Metrics = metrics;
    }
}

/// <summary>
/// Startup failed event args
/// </summary>
public class StartupFailedEventArgs : EventArgs
{
    public Exception Exception { get; }
    public string Phase { get; }
    public TimeSpan Elapsed { get; }

    public StartupFailedEventArgs(Exception exception, string phase, TimeSpan elapsed)
    {
        Exception = exception;
        Phase = phase;
        Elapsed = elapsed;
    }
}

/// <summary>
/// Startup performance metrics
/// </summary>
public class StartupMetrics
{
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan HostStartupDuration { get; set; }
    public TimeSpan ServiceResolutionDuration { get; set; }
    public TimeSpan ThemeInitializationDuration { get; set; }
    public TimeSpan BootscreenDuration { get; set; }
    public TimeSpan DataLoadingDuration { get; set; }
    public TimeSpan DatabaseInitializationDuration { get; set; }
    public int ServicesResolved { get; set; }
    public long MemoryUsed { get; set; }
    public bool DatabaseMigrationPerformed { get; set; }
    public bool DataSeedingPerformed { get; set; }
}
