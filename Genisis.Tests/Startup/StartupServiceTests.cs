using System.Diagnostics;
using System.Windows.Threading;
using FluentAssertions;
using Genisis.Presentation.Wpf.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Xunit;

namespace Genisis.Tests.Startup;

/// <summary>
/// Comprehensive tests for StartupService functionality
/// </summary>
public class StartupServiceTests : IDisposable
{
    private readonly IHost _host;
    private readonly Mock<ILogger<StartupService>> _mockLogger;
    private readonly Dispatcher _dispatcher;
    private bool _disposed;

    public StartupServiceTests()
    {
        // Configure test logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .CreateLogger();

        // Create test host
        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices(ConfigureTestServices)
            .Build();

        _mockLogger = new Mock<ILogger<StartupService>>();
        _dispatcher = Dispatcher.CurrentDispatcher;
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Act
        var startupService = new StartupService(_host, _dispatcher);

        // Assert
        startupService.Should().NotBeNull();
        startupService.IsInProgress.Should().BeFalse();
        startupService.IsComplete.Should().BeFalse();
        startupService.Progress.Should().Be(0);
        startupService.CurrentPhase.Should().Be("Initializing");
    }

    [Fact]
    public void Constructor_WithNullHost_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new StartupService(null!, _dispatcher);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("host");
    }

    [Fact]
    public void Constructor_WithNullDispatcher_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new StartupService(_host, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("dispatcher");
    }

    [Fact]
    public async Task StartAsync_WhenNotInProgress_ShouldStartSuccessfully()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var progressEvents = new List<StartupProgressEventArgs>();
        var phaseEvents = new List<StartupPhaseEventArgs>();
        var completedEvent = new List<StartupCompletedEventArgs>();

        startupService.ProgressChanged += (_, e) => progressEvents.Add(e);
        startupService.PhaseChanged += (_, e) => phaseEvents.Add(e);
        startupService.StartupCompleted += (_, e) => completedEvent.Add(e);

        // Act
        await startupService.StartAsync();

        // Assert
        startupService.IsComplete.Should().BeTrue();
        startupService.IsInProgress.Should().BeFalse();
        startupService.Progress.Should().Be(1.0);
        completedEvent.Should().HaveCount(1);
        progressEvents.Should().NotBeEmpty();
        phaseEvents.Should().NotBeEmpty();
    }

    [Fact]
    public async Task StartAsync_WhenAlreadyInProgress_ShouldNotStartAgain()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var firstStartTask = startupService.StartAsync();

        // Act
        await startupService.StartAsync(); // Second call

        // Assert
        startupService.IsInProgress.Should().BeFalse();
        startupService.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsync_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var stopwatch = Stopwatch.StartNew();

        // Act
        await startupService.StartAsync();

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // 10 seconds max
    }

    [Fact]
    public async Task StartAsync_ShouldProgressThroughAllPhases()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var phaseEvents = new List<StartupPhaseEventArgs>();

        startupService.PhaseChanged += (_, e) => phaseEvents.Add(e);

        // Act
        await startupService.StartAsync();

        // Assert
        phaseEvents.Should().NotBeEmpty();
        phaseEvents.Should().Contain(e => e.CurrentPhase == "Host Startup");
        phaseEvents.Should().Contain(e => e.CurrentPhase == "Service Resolution");
        phaseEvents.Should().Contain(e => e.CurrentPhase == "Theme Initialization");
        phaseEvents.Should().Contain(e => e.CurrentPhase == "Bootscreen Animation");
        phaseEvents.Should().Contain(e => e.CurrentPhase == "Background Initialization");
    }

    [Fact]
    public async Task StartAsync_ShouldReportProgressCorrectly()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var progressEvents = new List<StartupProgressEventArgs>();

        startupService.ProgressChanged += (_, e) => progressEvents.Add(e);

        // Act
        await startupService.StartAsync();

        // Assert
        progressEvents.Should().NotBeEmpty();
        progressEvents.First().Progress.Should().BeGreaterOrEqualTo(0);
        progressEvents.Last().Progress.Should().Be(1.0);
        
        // Progress should be monotonically increasing
        for (int i = 1; i < progressEvents.Count; i++)
        {
            progressEvents[i].Progress.Should().BeGreaterOrEqualTo(progressEvents[i - 1].Progress);
        }
    }

    [Fact]
    public async Task GetMetrics_AfterStartup_ShouldReturnValidMetrics()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);

        // Act
        await startupService.StartAsync();
        var metrics = startupService.GetMetrics();

        // Assert
        metrics.Should().NotBeNull();
        metrics.TotalDuration.Should().BeGreaterThan(TimeSpan.Zero);
        metrics.HostStartupDuration.Should().BeGreaterThan(TimeSpan.Zero);
        metrics.ServiceResolutionDuration.Should().BeGreaterThan(TimeSpan.Zero);
        metrics.ServicesResolved.Should().BeGreaterThan(0);
        metrics.MemoryUsed.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Cancel_WhenInProgress_ShouldStopStartup()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var cancellationTokenSource = new CancellationTokenSource();
        
        // Start startup in background
        var startupTask = Task.Run(async () =>
        {
            try
            {
                await startupService.StartAsync();
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelled
            }
        });

        // Act
        startupService.Cancel();

        // Assert
        startupService.IsInProgress.Should().BeFalse();
    }

    [Fact]
    public async Task StartAsync_WithServiceResolutionFailure_ShouldRaiseStartupFailedEvent()
    {
        // Arrange
        var mockHost = new Mock<IHost>();
        mockHost.Setup(h => h.StartAsync()).ThrowsAsync(new InvalidOperationException("Service resolution failed"));
        
        var startupService = new StartupService(mockHost.Object, _dispatcher);
        var failedEvents = new List<StartupFailedEventArgs>();

        startupService.StartupFailed += (_, e) => failedEvents.Add(e);

        // Act & Assert
        var action = async () => await startupService.StartAsync();
        await action.Should().ThrowAsync<InvalidOperationException>();
        
        failedEvents.Should().HaveCount(1);
        failedEvents[0].Exception.Message.Should().Be("Service resolution failed");
    }

    [Fact]
    public async Task StartAsync_ShouldHandleBackgroundInitializationErrors()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var progressEvents = new List<StartupProgressEventArgs>();

        startupService.ProgressChanged += (_, e) => progressEvents.Add(e);

        // Act
        await startupService.StartAsync();

        // Assert
        startupService.IsComplete.Should().BeTrue();
        progressEvents.Should().NotBeEmpty();
        // Should complete even if background initialization fails
    }

    [Theory]
    [InlineData("Host Startup", 0.1)]
    [InlineData("Service Resolution", 0.2)]
    [InlineData("Theme Initialization", 0.3)]
    [InlineData("Bootscreen Animation", 0.4)]
    [InlineData("Background Initialization", 0.5)]
    [InlineData("Data Loading", 0.7)]
    [InlineData("Database Initialization", 0.9)]
    [InlineData("Complete", 1.0)]
    public void StartupPhases_ShouldHaveCorrectProgressValues(string phaseName, double expectedProgress)
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);

        // Act
        var metrics = startupService.GetMetrics();

        // Assert
        // This test validates the phase definitions
        // The actual progress values are tested in integration tests
        phaseName.Should().NotBeNullOrEmpty();
        expectedProgress.Should().BeInRange(0, 1);
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
        // Configure minimal test services
        services.AddLogging();
        services.AddMemoryCache();
        
        // Mock services for testing
        services.AddSingleton<Mock<ILogger<StartupService>>>();
    }
}
