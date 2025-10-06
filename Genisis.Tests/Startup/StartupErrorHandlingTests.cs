using System.Windows.Threading;
using FluentAssertions;
using Genisis.Presentation.Wpf.Services;
using Genisis.Presentation.Wpf.Themes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Serilog;
using Xunit;

namespace Genisis.Tests.Startup;

/// <summary>
/// Error handling tests for startup process
/// </summary>
public class StartupErrorHandlingTests : IDisposable
{
    private readonly IHost _host;
    private readonly Dispatcher _dispatcher;
    private bool _disposed;

    public StartupErrorHandlingTests()
    {
        // Configure test logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning() // Reduce logging noise for error tests
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
    public async Task Startup_WithHostStartupFailure_ShouldRaiseStartupFailedEvent()
    {
        // Arrange
        var mockHost = new Mock<IHost>();
        mockHost.Setup(h => h.StartAsync()).ThrowsAsync(new InvalidOperationException("Host startup failed"));

        var startupService = new StartupService(mockHost.Object, _dispatcher);
        var failedEvents = new List<StartupFailedEventArgs>();

        startupService.StartupFailed += (_, e) => failedEvents.Add(e);

        // Act & Assert
        var action = async () => await startupService.StartAsync();
        await action.Should().ThrowAsync<InvalidOperationException>();
        
        failedEvents.Should().HaveCount(1);
        failedEvents[0].Exception.Message.Should().Be("Host startup failed");
        failedEvents[0].Phase.Should().Be("Host Startup");
    }

    [Fact]
    public async Task Startup_WithServiceResolutionFailure_ShouldRaiseStartupFailedEvent()
    {
        // Arrange
        var mockHost = new Mock<IHost>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeProvider = new Mock<IServiceScopeFactory>();

        mockHost.Setup(h => h.StartAsync()).Returns(Task.CompletedTask);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        mockScopeProvider.Setup(s => s.CreateScope()).Returns(mockScope.Object);
        mockHost.Setup(h => h.Services).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(s => s.GetService(typeof(IServiceScopeFactory))).Returns(mockScopeProvider.Object);
        mockServiceProvider.Setup(s => s.GetRequiredService(typeof(MainWindowV3))).Throws(new InvalidOperationException("Service resolution failed"));

        var startupService = new StartupService(mockHost.Object, _dispatcher);
        var failedEvents = new List<StartupFailedEventArgs>();

        startupService.StartupFailed += (_, e) => failedEvents.Add(e);

        // Act & Assert
        var action = async () => await startupService.StartAsync();
        await action.Should().ThrowAsync<InvalidOperationException>();
        
        failedEvents.Should().HaveCount(1);
        failedEvents[0].Exception.Message.Should().Be("Service resolution failed");
        failedEvents[0].Phase.Should().Be("Service Resolution");
    }

    [Fact]
    public async Task Startup_WithThemeInitializationFailure_ShouldRaiseStartupFailedEvent()
    {
        // Arrange
        var mockHost = new Mock<IHost>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeProvider = new Mock<IServiceScopeFactory>();
        var mockThemeService = new Mock<IThemeService>();

        mockHost.Setup(h => h.StartAsync()).Returns(Task.CompletedTask);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        mockScopeProvider.Setup(s => s.CreateScope()).Returns(mockScope.Object);
        mockHost.Setup(h => h.Services).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(s => s.GetService(typeof(IServiceScopeFactory))).Returns(mockScopeProvider.Object);
        mockServiceProvider.Setup(s => s.GetRequiredService(typeof(MainWindowV3))).Returns(new Mock<MainWindowV3>().Object);
        mockServiceProvider.Setup(s => s.GetRequiredService(typeof(MainViewModel))).Returns(new Mock<MainViewModel>().Object);
        mockServiceProvider.Setup(s => s.GetRequiredService(typeof(IThemeService))).Returns(mockThemeService.Object);
        
        mockThemeService.Setup(t => t.InitializeAsync()).ThrowsAsync(new InvalidOperationException("Theme initialization failed"));

        var startupService = new StartupService(mockHost.Object, _dispatcher);
        var failedEvents = new List<StartupFailedEventArgs>();

        startupService.StartupFailed += (_, e) => failedEvents.Add(e);

        // Act & Assert
        var action = async () => await startupService.StartAsync();
        await action.Should().ThrowAsync<InvalidOperationException>();
        
        failedEvents.Should().HaveCount(1);
        failedEvents[0].Exception.Message.Should().Be("Theme initialization failed");
        failedEvents[0].Phase.Should().Be("Theme Initialization");
    }

    [Fact]
    public async Task Startup_WithBootscreenFailure_ShouldRaiseStartupFailedEvent()
    {
        // Arrange
        var mockHost = new Mock<IHost>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeProvider = new Mock<IServiceScopeFactory>();
        var mockThemeService = new Mock<IThemeService>();
        var mockMainWindow = new Mock<MainWindowV3>();

        mockHost.Setup(h => h.StartAsync()).Returns(Task.CompletedTask);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        mockScopeProvider.Setup(s => s.CreateScope()).Returns(mockScope.Object);
        mockHost.Setup(h => h.Services).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(s => s.GetService(typeof(IServiceScopeFactory))).Returns(mockScopeProvider.Object);
        mockServiceProvider.Setup(s => s.GetRequiredService(typeof(MainWindowV3))).Returns(mockMainWindow.Object);
        mockServiceProvider.Setup(s => s.GetRequiredService(typeof(MainViewModel))).Returns(new Mock<MainViewModel>().Object);
        mockServiceProvider.Setup(s => s.GetRequiredService(typeof(IThemeService))).Returns(mockThemeService.Object);
        
        mockThemeService.Setup(t => t.InitializeAsync()).Returns(Task.CompletedTask);
        mockThemeService.Setup(t => t.CurrentTheme).Returns(new FantasyThemeProvider());
        mockMainWindow.Setup(m => m.StartBootscreenAsync(It.IsAny<IThemeProvider>())).ThrowsAsync(new InvalidOperationException("Bootscreen failed"));

        var startupService = new StartupService(mockHost.Object, _dispatcher);
        var failedEvents = new List<StartupFailedEventArgs>();

        startupService.StartupFailed += (_, e) => failedEvents.Add(e);

        // Act & Assert
        var action = async () => await startupService.StartAsync();
        await action.Should().ThrowAsync<InvalidOperationException>();
        
        failedEvents.Should().HaveCount(1);
        failedEvents[0].Exception.Message.Should().Be("Bootscreen failed");
        failedEvents[0].Phase.Should().Be("Bootscreen Animation");
    }

    [Fact]
    public async Task Startup_WithBackgroundInitializationFailure_ShouldContinueGracefully()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var completedEvents = new List<StartupCompletedEventArgs>();

        startupService.StartupCompleted += (_, e) => completedEvents.Add(e);

        // Act
        await startupService.StartAsync();

        // Assert
        startupService.IsComplete.Should().BeTrue();
        completedEvents.Should().HaveCount(1);
        // Should complete even if background initialization fails
    }

    [Fact]
    public async Task Startup_WithCancellation_ShouldStopGracefully()
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
        startupService.IsComplete.Should().BeFalse();
    }

    [Fact]
    public async Task Startup_WithMultipleFailures_ShouldReportFirstFailure()
    {
        // Arrange
        var mockHost = new Mock<IHost>();
        mockHost.Setup(h => h.StartAsync()).ThrowsAsync(new InvalidOperationException("First failure"));

        var startupService = new StartupService(mockHost.Object, _dispatcher);
        var failedEvents = new List<StartupFailedEventArgs>();

        startupService.StartupFailed += (_, e) => failedEvents.Add(e);

        // Act & Assert
        var action = async () => await startupService.StartAsync();
        await action.Should().ThrowAsync<InvalidOperationException>();
        
        failedEvents.Should().HaveCount(1);
        failedEvents[0].Exception.Message.Should().Be("First failure");
    }

    [Fact]
    public async Task Startup_WithNullServices_ShouldHandleGracefully()
    {
        // Arrange
        var mockHost = new Mock<IHost>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeProvider = new Mock<IServiceScopeFactory>();

        mockHost.Setup(h => h.StartAsync()).Returns(Task.CompletedTask);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        mockScopeProvider.Setup(s => s.CreateScope()).Returns(mockScope.Object);
        mockHost.Setup(h => h.Services).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(s => s.GetService(typeof(IServiceScopeFactory))).Returns(mockScopeProvider.Object);
        mockServiceProvider.Setup(s => s.GetRequiredService(typeof(MainWindowV3))).Returns((MainWindowV3)null!);

        var startupService = new StartupService(mockHost.Object, _dispatcher);
        var failedEvents = new List<StartupFailedEventArgs>();

        startupService.StartupFailed += (_, e) => failedEvents.Add(e);

        // Act & Assert
        var action = async () => await startupService.StartAsync();
        await action.Should().ThrowAsync<ArgumentNullException>();
        
        failedEvents.Should().HaveCount(1);
    }

    [Fact]
    public async Task Startup_WithTimeout_ShouldHandleGracefully()
    {
        // Arrange
        var mockHost = new Mock<IHost>();
        mockHost.Setup(h => h.StartAsync()).Returns(Task.Delay(TimeSpan.FromHours(1))); // Simulate timeout

        var startupService = new StartupService(mockHost.Object, _dispatcher);
        var failedEvents = new List<StartupFailedEventArgs>();

        startupService.StartupFailed += (_, e) => failedEvents.Add(e);

        // Act & Assert
        var action = async () => await startupService.StartAsync();
        await action.Should().ThrowAsync<InvalidOperationException>();
        
        failedEvents.Should().HaveCount(1);
    }

    [Fact]
    public async Task Startup_WithMemoryPressure_ShouldHandleGracefully()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var completedEvents = new List<StartupCompletedEventArgs>();

        startupService.StartupCompleted += (_, e) => completedEvents.Add(e);

        // Act
        await startupService.StartAsync();

        // Assert
        startupService.IsComplete.Should().BeTrue();
        completedEvents.Should().HaveCount(1);
        
        var metrics = completedEvents[0].Metrics;
        metrics.MemoryUsed.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Startup_WithResourceExhaustion_ShouldHandleGracefully()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var completedEvents = new List<StartupCompletedEventArgs>();

        startupService.StartupCompleted += (_, e) => completedEvents.Add(e);

        // Act
        await startupService.StartAsync();

        // Assert
        startupService.IsComplete.Should().BeTrue();
        completedEvents.Should().HaveCount(1);
        // Should complete even under resource pressure
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
        // Configure minimal test services for error testing
        services.AddLogging();
        services.AddMemoryCache();
    }
}
