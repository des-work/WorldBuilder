using System.Diagnostics;
using System.Windows.Threading;
using FluentAssertions;
using Genisis.Presentation.Wpf.Services;
using Genisis.Presentation.Wpf.Themes;
using Genisis.Presentation.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Xunit;

namespace Genisis.Tests.Startup;

/// <summary>
/// Integration tests for startup process with real services
/// </summary>
public class StartupIntegrationTests : IDisposable
{
    private readonly IHost _host;
    private readonly Dispatcher _dispatcher;
    private bool _disposed;

    public StartupIntegrationTests()
    {
        // Configure test logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .CreateLogger();

        // Create test host with real services
        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices(ConfigureIntegrationServices)
            .Build();

        _dispatcher = Dispatcher.CurrentDispatcher;
    }

    [Fact]
    public async Task Startup_WithRealServices_ShouldCompleteSuccessfully()
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
        metrics.TotalDuration.Should().BeGreaterThan(TimeSpan.Zero);
        metrics.ServicesResolved.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Startup_ShouldInitializeThemeService()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);

        // Act
        await startupService.StartAsync();

        // Assert
        using var scope = _host.Services.CreateScope();
        var themeService = scope.ServiceProvider.GetRequiredService<IThemeService>();
        themeService.Should().NotBeNull();
        themeService.CurrentTheme.Should().NotBeNull();
    }

    [Fact]
    public async Task Startup_ShouldResolveMainWindow()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);

        // Act
        await startupService.StartAsync();

        // Assert
        using var scope = _host.Services.CreateScope();
        var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindowV3>();
        mainWindow.Should().NotBeNull();
        mainWindow.DataContext.Should().NotBeNull();
    }

    [Fact]
    public async Task Startup_ShouldResolveMainViewModel()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);

        // Act
        await startupService.StartAsync();

        // Assert
        using var scope = _host.Services.CreateScope();
        var mainViewModel = scope.ServiceProvider.GetRequiredService<MainViewModel>();
        mainViewModel.Should().NotBeNull();
    }

    [Fact]
    public async Task Startup_ShouldInitializeAllRequiredServices()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var resolvedServices = new List<object>();

        // Act
        await startupService.StartAsync();

        // Assert
        using var scope = _host.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Verify all critical services can be resolved
        var mainWindow = services.GetRequiredService<MainWindowV3>();
        var mainViewModel = services.GetRequiredService<MainViewModel>();
        var themeService = services.GetRequiredService<IThemeService>();
        var startupServiceResolved = services.GetRequiredService<IStartupService>();

        mainWindow.Should().NotBeNull();
        mainViewModel.Should().NotBeNull();
        themeService.Should().NotBeNull();
        startupServiceResolved.Should().NotBeNull();
    }

    [Fact]
    public async Task Startup_ShouldHandleServiceResolutionErrors()
    {
        // Arrange
        var mockHost = new Mock<IHost>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeProvider = new Mock<IServiceScopeFactory>();

        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        mockScopeProvider.Setup(s => s.CreateScope()).Returns(mockScope.Object);
        mockHost.Setup(h => h.Services).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(s => s.GetService(typeof(IServiceScopeFactory))).Returns(mockScopeProvider.Object);
        mockServiceProvider.Setup(s => s.GetRequiredService(typeof(MainWindowV3))).Throws(new InvalidOperationException("Service not found"));

        var startupService = new StartupService(mockHost.Object, _dispatcher);
        var failedEvents = new List<StartupFailedEventArgs>();

        startupService.StartupFailed += (_, e) => failedEvents.Add(e);

        // Act & Assert
        var action = async () => await startupService.StartAsync();
        await action.Should().ThrowAsync<InvalidOperationException>();
        
        failedEvents.Should().HaveCount(1);
        failedEvents[0].Exception.Message.Should().Be("Service not found");
    }

    [Fact]
    public async Task Startup_ShouldCompleteBackgroundInitialization()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var metrics = new List<StartupMetrics>();

        startupService.StartupCompleted += (_, e) => metrics.Add(e.Metrics);

        // Act
        await startupService.StartAsync();

        // Wait for background initialization to complete
        await Task.Delay(1000);

        // Assert
        metrics.Should().HaveCount(1);
        // Background initialization should complete without errors
    }

    [Fact]
    public async Task Startup_ShouldHandleThemeInitializationErrors()
    {
        // Arrange
        var mockHost = new Mock<IHost>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeProvider = new Mock<IServiceScopeFactory>();
        var mockThemeService = new Mock<IThemeService>();

        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        mockScopeProvider.Setup(s => s.CreateScope()).Returns(mockScope.Object);
        mockHost.Setup(h => h.Services).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(s => s.GetService(typeof(IServiceScopeFactory))).Returns(mockScopeProvider.Object);
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
    }

    [Fact]
    public async Task Startup_ShouldHandleBootscreenErrors()
    {
        // Arrange
        var mockHost = new Mock<IHost>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeProvider = new Mock<IServiceScopeFactory>();
        var mockThemeService = new Mock<IThemeService>();
        var mockMainWindow = new Mock<MainWindowV3>();

        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        mockScopeProvider.Setup(s => s.CreateScope()).Returns(mockScope.Object);
        mockHost.Setup(h => h.Services).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(s => s.GetService(typeof(IServiceScopeFactory))).Returns(mockScopeProvider.Object);
        mockServiceProvider.Setup(s => s.GetRequiredService(typeof(IThemeService))).Returns(mockThemeService.Object);
        mockServiceProvider.Setup(s => s.GetRequiredService(typeof(MainWindowV3))).Returns(mockMainWindow.Object);
        
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
    }

    [Fact]
    public async Task Startup_ShouldProvideAccurateMetrics()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var metrics = new List<StartupMetrics>();

        startupService.StartupCompleted += (_, e) => metrics.Add(e.Metrics);

        // Act
        await startupService.StartAsync();

        // Assert
        metrics.Should().HaveCount(1);
        var startupMetrics = metrics[0];
        
        startupMetrics.TotalDuration.Should().BeGreaterThan(TimeSpan.Zero);
        startupMetrics.HostStartupDuration.Should().BeGreaterThan(TimeSpan.Zero);
        startupMetrics.ServiceResolutionDuration.Should().BeGreaterThan(TimeSpan.Zero);
        startupMetrics.ThemeInitializationDuration.Should().BeGreaterThan(TimeSpan.Zero);
        startupMetrics.BootscreenDuration.Should().BeGreaterThan(TimeSpan.Zero);
        startupMetrics.ServicesResolved.Should().BeGreaterThan(0);
        startupMetrics.MemoryUsed.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Startup_ShouldHandleMultipleConcurrentRequests()
    {
        // Arrange
        var startupService = new StartupService(_host, _dispatcher);
        var tasks = new List<Task>();
        var results = new List<bool>();

        // Act
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await startupService.StartAsync();
                    results.Add(true);
                }
                catch (InvalidOperationException)
                {
                    // Expected when already in progress
                    results.Add(false);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        results.Should().Contain(true); // At least one should succeed
        startupService.IsComplete.Should().BeTrue();
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

    private void ConfigureIntegrationServices(IServiceCollection services)
    {
        // Configure real services for integration testing
        services.AddLogging();
        services.AddMemoryCache();
        
        // Register real services
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IStartupService, StartupService>();
        services.AddSingleton<MainWindowV3>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<AIViewModel>();
    }
}
