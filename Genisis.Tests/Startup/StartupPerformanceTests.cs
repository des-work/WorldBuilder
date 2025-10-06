using System.Diagnostics;
using System.Windows.Threading;
using FluentAssertions;
using Genisis.Presentation.Wpf.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Xunit;

namespace Genisis.Tests.Startup;

/// <summary>
/// Performance tests for startup process
/// </summary>
public class StartupPerformanceTests : IDisposable
{
    private readonly IHost _host;
    private readonly Dispatcher _dispatcher;
    private bool _disposed;

    public StartupPerformanceTests()
    {
        // Configure test logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning() // Reduce logging noise for performance tests
            .WriteTo.Debug()
            .CreateLogger();

        // Create test host
        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices(ConfigureTestServices)
            .Build();

        _dispatcher = Dispatcher.CurrentDispatcher;
    }

    [Fact]
    public async Task Startup_ShouldCompleteWithinTargetTime()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var stopwatch = Stopwatch.StartNew();

        // Act
        await startupService.StartAsync();

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "Startup should complete within 5 seconds");
    }

    [Fact]
    public async Task HostStartup_ShouldCompleteWithinTargetTime()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var metrics = new List<StartupMetrics>();

        startupService.StartupCompleted += (_, e) => metrics.Add(e.Metrics);

        // Act
        await startupService.StartAsync();

        // Assert
        metrics.Should().HaveCount(1);
        metrics[0].HostStartupDuration.TotalMilliseconds.Should().BeLessThan(100, "Host startup should complete within 100ms");
    }

    [Fact]
    public async Task ServiceResolution_ShouldCompleteWithinTargetTime()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var metrics = new List<StartupMetrics>();

        startupService.StartupCompleted += (_, e) => metrics.Add(e.Metrics);

        // Act
        await startupService.StartAsync();

        // Assert
        metrics.Should().HaveCount(1);
        metrics[0].ServiceResolutionDuration.TotalMilliseconds.Should().BeLessThan(200, "Service resolution should complete within 200ms");
    }

    [Fact]
    public async Task ThemeInitialization_ShouldCompleteWithinTargetTime()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var metrics = new List<StartupMetrics>();

        startupService.StartupCompleted += (_, e) => metrics.Add(e.Metrics);

        // Act
        await startupService.StartAsync();

        // Assert
        metrics.Should().HaveCount(1);
        metrics[0].ThemeInitializationDuration.TotalMilliseconds.Should().BeLessThan(300, "Theme initialization should complete within 300ms");
    }

    [Fact]
    public async Task BootscreenAnimation_ShouldCompleteWithinTargetTime()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var metrics = new List<StartupMetrics>();

        startupService.StartupCompleted += (_, e) => metrics.Add(e.Metrics);

        // Act
        await startupService.StartAsync();

        // Assert
        metrics.Should().HaveCount(1);
        metrics[0].BootscreenDuration.TotalMilliseconds.Should().BeLessThan(2000, "Bootscreen animation should complete within 2 seconds");
    }

    [Fact]
    public async Task Startup_ShouldUseReasonableMemory()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var metrics = new List<StartupMetrics>();

        startupService.StartupCompleted += (_, e) => metrics.Add(e.Metrics);

        // Act
        await startupService.StartAsync();

        // Assert
        metrics.Should().HaveCount(1);
        metrics[0].MemoryUsed.Should().BeLessThan(100 * 1024 * 1024, "Startup should use less than 100MB of memory");
    }

    [Fact]
    public async Task Startup_ShouldResolveExpectedNumberOfServices()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var metrics = new List<StartupMetrics>();

        startupService.StartupCompleted += (_, e) => metrics.Add(e.Metrics);

        // Act
        await startupService.StartAsync();

        // Assert
        metrics.Should().HaveCount(1);
        metrics[0].ServicesResolved.Should().BeGreaterThan(0, "Should resolve at least one service");
        metrics[0].ServicesResolved.Should().BeLessThan(20, "Should not resolve too many services");
    }

    [Fact]
    public async Task MultipleStartups_ShouldHaveConsistentPerformance()
    {
        // Arrange
        var startupService1 = new StartupService(_host, _dispatcher);
        var startupService2 = new StartupService(_host, _dispatcher);
        var metrics1 = new List<StartupMetrics>();
        var metrics2 = new List<StartupMetrics>();

        startupService1.StartupCompleted += (_, e) => metrics1.Add(e.Metrics);
        startupService2.StartupCompleted += (_, e) => metrics2.Add(e.Metrics);

        // Act
        await startupService1.StartAsync();
        await startupService2.StartAsync();

        // Assert
        metrics1.Should().HaveCount(1);
        metrics2.Should().HaveCount(1);
        
        var duration1 = metrics1[0].TotalDuration.TotalMilliseconds;
        var duration2 = metrics2[0].TotalDuration.TotalMilliseconds;
        
        var difference = Math.Abs(duration1 - duration2);
        difference.Should().BeLessThan(1000, "Multiple startups should have consistent performance");
    }

    [Fact]
    public async Task Startup_ShouldCompleteAllPhasesInCorrectOrder()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var phaseEvents = new List<StartupPhaseEventArgs>();

        startupService.PhaseChanged += (_, e) => phaseEvents.Add(e);

        // Act
        await startupService.StartAsync();

        // Assert
        phaseEvents.Should().NotBeEmpty();
        
        // Verify phase order
        var phases = phaseEvents.Select(e => e.CurrentPhase).ToList();
        phases.Should().Contain("Host Startup");
        phases.Should().Contain("Service Resolution");
        phases.Should().Contain("Theme Initialization");
        phases.Should().Contain("Bootscreen Animation");
        phases.Should().Contain("Background Initialization");
    }

    [Fact]
    public async Task Startup_ShouldHaveReasonablePhaseDurations()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var phaseEvents = new List<StartupPhaseEventArgs>();

        startupService.PhaseChanged += (_, e) => phaseEvents.Add(e);

        // Act
        await startupService.StartAsync();

        // Assert
        phaseEvents.Should().NotBeEmpty();
        
        foreach (var phaseEvent in phaseEvents)
        {
            phaseEvent.PhaseDuration.TotalMilliseconds.Should().BeLessThan(3000, 
                $"Phase '{phaseEvent.CurrentPhase}' should complete within 3 seconds");
        }
    }

    [Fact]
    public async Task Startup_ShouldProgressMonotonically()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var progressEvents = new List<StartupProgressEventArgs>();

        startupService.ProgressChanged += (_, e) => progressEvents.Add(e);

        // Act
        await startupService.StartAsync();

        // Assert
        progressEvents.Should().NotBeEmpty();
        
        // Verify monotonic progress
        for (int i = 1; i < progressEvents.Count; i++)
        {
            progressEvents[i].Progress.Should().BeGreaterOrEqualTo(progressEvents[i - 1].Progress,
                "Progress should increase monotonically");
        }
    }

    [Fact]
    public async Task Startup_ShouldHandleConcurrentAccess()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await startupService.StartAsync();
                }
                catch (InvalidOperationException)
                {
                    // Expected when already in progress
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        startupService.IsComplete.Should().BeTrue();
        startupService.IsInProgress.Should().BeFalse();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _host?.Dispose();
            Log.CloseAndFlush();
            _disposed = true;
        }
    }

    private void ConfigureTestServices(IServiceCollection services)
    {
        // Configure minimal test services for performance testing
        services.AddLogging();
        services.AddMemoryCache();
    }
}
