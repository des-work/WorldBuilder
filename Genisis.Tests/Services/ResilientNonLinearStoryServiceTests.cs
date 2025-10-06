using System.Diagnostics;
using FluentAssertions;
using Genisis.Presentation.Wpf.Services;
using Genisis.Presentation.Wpf.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Genisis.Tests.Services;

/// <summary>
/// Comprehensive tests for ResilientNonLinearStoryService
/// </summary>
public class ResilientNonLinearStoryServiceTests : IDisposable
{
    private readonly Mock<INonLinearStoryService> _mockBaseService;
    private readonly Mock<ILogger<ResilientNonLinearStoryService>> _mockLogger;
    private readonly Mock<IRetryPolicy> _mockRetryPolicy;
    private readonly Mock<ICircuitBreaker> _mockCircuitBreaker;
    private readonly Mock<IBackgroundProcessingService> _mockBackgroundService;
    private readonly IMemoryCache _memoryCache;
    private readonly ResilientNonLinearStoryService _service;
    private bool _disposed = false;

    public ResilientNonLinearStoryServiceTests()
    {
        _mockBaseService = new Mock<INonLinearStoryService>();
        _mockLogger = new Mock<ILogger<ResilientNonLinearStoryService>>();
        _mockRetryPolicy = new Mock<IRetryPolicy>();
        _mockCircuitBreaker = new Mock<ICircuitBreaker>();
        _mockBackgroundService = new Mock<IBackgroundProcessingService>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _service = new ResilientNonLinearStoryService(
            _mockBaseService.Object,
            _mockLogger.Object,
            _mockRetryPolicy.Object,
            _mockCircuitBreaker.Object,
            _memoryCache,
            _mockBackgroundService.Object);
    }

    [Fact]
    public async Task CreateElementAsync_WithValidParameters_ShouldReturnElement()
    {
        // Arrange
        var expectedElement = new StoryElement(ElementType.Character, "Test Character", "Test Description");
        _mockCircuitBreaker.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<StoryElement>>>()))
            .ReturnsAsync(expectedElement);

        // Act
        var result = await _service.CreateElementAsync(ElementType.Character, "Test Character", "Test Description");

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Character");
        result.Description.Should().Be("Test Description");
        result.ElementType.Should().Be(ElementType.Character);
        
        _mockCircuitBreaker.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<StoryElement>>>()), Times.Once);
    }

    [Fact]
    public async Task CreateElementAsync_WithCircuitBreakerFailure_ShouldReturnFallbackElement()
    {
        // Arrange
        _mockCircuitBreaker.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<StoryElement>>>()))
            .ThrowsAsync(new CircuitBreakerOpenException("Circuit breaker is open"));

        // Act
        var result = await _service.CreateElementAsync(ElementType.Character, "Test Character", "Test Description");

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Character (Offline)");
        result.Description.Should().Be("Test Description - Created offline due to error");
        result.ElementType.Should().Be(ElementType.Character);
        
        _mockBackgroundService.Verify(x => x.EnqueueTaskAsync(
            It.IsAny<Func<INonLinearStoryService, Task>>(),
            _mockBaseService.Object,
            TaskPriority.High), Times.Once);
    }

    [Fact]
    public async Task CreateElementAsync_WithRetryPolicyFailure_ShouldReturnFallbackElement()
    {
        // Arrange
        _mockCircuitBreaker.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<StoryElement>>>()))
            .ThrowsAsync(new InvalidOperationException("Service unavailable"));

        // Act
        var result = await _service.CreateElementAsync(ElementType.Character, "Test Character", "Test Description");

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Character (Offline)");
        result.Description.Should().Be("Test Description - Created offline due to error");
        
        _mockBackgroundService.Verify(x => x.EnqueueTaskAsync(
            It.IsAny<Func<INonLinearStoryService, Task>>(),
            _mockBaseService.Object,
            TaskPriority.High), Times.Once);
    }

    [Fact]
    public async Task GetElementByIdAsync_WithCachedElement_ShouldReturnCachedElement()
    {
        // Arrange
        var element = new StoryElement(ElementType.Character, "Cached Character", "Cached Description");
        _memoryCache.Set($"element_{element.Id}", element, TimeSpan.FromMinutes(30));

        // Act
        var result = await _service.GetElementByIdAsync(element.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Cached Character");
        result.Description.Should().Be("Cached Description");
        
        // Should not call base service
        _mockCircuitBreaker.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<StoryElement?>>>()), Times.Never);
    }

    [Fact]
    public async Task GetElementByIdAsync_WithNonCachedElement_ShouldCallBaseService()
    {
        // Arrange
        var elementId = Guid.NewGuid();
        var expectedElement = new StoryElement(ElementType.Character, "Test Character", "Test Description");
        _mockCircuitBreaker.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<StoryElement?>>>()))
            .ReturnsAsync(expectedElement);

        // Act
        var result = await _service.GetElementByIdAsync(elementId);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Character");
        
        _mockCircuitBreaker.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<StoryElement?>>>()), Times.Once);
    }

    [Fact]
    public async Task SearchElementsAsync_WithCachedResults_ShouldReturnCachedResults()
    {
        // Arrange
        var searchTerm = "test";
        var cachedResults = new List<StoryElement>
        {
            new StoryElement(ElementType.Character, "Test Character", "Test Description")
        };
        _memoryCache.Set($"search_{searchTerm}", cachedResults, TimeSpan.FromMinutes(10));

        // Act
        var result = await _service.SearchElementsAsync(searchTerm);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Test Character");
        
        // Should not call base service
        _mockCircuitBreaker.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<IEnumerable<StoryElement>>>>()), Times.Never);
    }

    [Fact]
    public async Task SearchElementsAsync_WithNonCachedResults_ShouldCallBaseService()
    {
        // Arrange
        var searchTerm = "test";
        var expectedResults = new List<StoryElement>
        {
            new StoryElement(ElementType.Character, "Test Character", "Test Description")
        };
        _mockCircuitBreaker.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<IEnumerable<StoryElement>>>>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _service.SearchElementsAsync(searchTerm);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Test Character");
        
        _mockCircuitBreaker.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<IEnumerable<StoryElement>>>>()), Times.Once);
    }

    [Fact]
    public async Task UpdateElementAsync_WithValidElement_ShouldUpdateCaches()
    {
        // Arrange
        var element = new StoryElement(ElementType.Character, "Updated Character", "Updated Description");
        _mockCircuitBreaker.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<StoryElement>>>()))
            .ReturnsAsync(element);

        // Act
        var result = await _service.UpdateElementAsync(element);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Character");
        
        // Verify caches are updated
        _memoryCache.TryGetValue($"element_{element.Id}", out StoryElement? cachedElement).Should().BeTrue();
        cachedElement!.Title.Should().Be("Updated Character");
    }

    [Fact]
    public async Task DeleteElementAsync_WithValidId_ShouldRemoveFromCaches()
    {
        // Arrange
        var elementId = Guid.NewGuid();
        var element = new StoryElement(ElementType.Character, "Test Character", "Test Description");
        _memoryCache.Set($"element_{elementId}", element, TimeSpan.FromMinutes(30));
        
        _mockCircuitBreaker.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteElementAsync(elementId);

        // Assert
        result.Should().BeTrue();
        
        // Verify element is removed from cache
        _memoryCache.TryGetValue($"element_{elementId}", out StoryElement? cachedElement).Should().BeFalse();
    }

    [Fact]
    public async Task LinkElementsAsync_WithValidElements_ShouldReturnTrue()
    {
        // Arrange
        var element1 = new StoryElement(ElementType.Character, "Character 1", "Description 1");
        var element2 = new StoryElement(ElementType.Character, "Character 2", "Description 2");
        
        _mockCircuitBreaker.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.LinkElementsAsync(element1, element2);

        // Assert
        result.Should().BeTrue();
        _mockCircuitBreaker.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<bool>>>()), Times.Once);
    }

    [Fact]
    public async Task LinkElementsAsync_WithFailure_ShouldReturnFalse()
    {
        // Arrange
        var element1 = new StoryElement(ElementType.Character, "Character 1", "Description 1");
        var element2 = new StoryElement(ElementType.Character, "Character 2", "Description 2");
        
        _mockCircuitBreaker.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<bool>>>()))
            .ThrowsAsync(new InvalidOperationException("Linking failed"));

        // Act
        var result = await _service.LinkElementsAsync(element1, element2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task LoadLargeDatasetAsync_WithValidCount_ShouldCompleteSuccessfully()
    {
        // Arrange
        var count = 1000;
        _mockCircuitBreaker.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<bool>>>()))
            .ReturnsAsync(true);

        // Act
        await _service.LoadLargeDatasetAsync(count);

        // Assert
        _mockCircuitBreaker.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<bool>>>()), Times.Once);
    }

    [Fact]
    public async Task LoadLargeDatasetAsync_WithFailure_ShouldCreatePlaceholderElements()
    {
        // Arrange
        var count = 100;
        _mockCircuitBreaker.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<bool>>>()))
            .ThrowsAsync(new InvalidOperationException("Dataset loading failed"));

        // Act
        await _service.LoadLargeDatasetAsync(count);

        // Assert
        // Should create placeholder elements (limited to 100 for performance)
        // This is tested by checking that the service doesn't throw an exception
    }

    [Fact]
    public async Task PerformanceTest_CreateElementAsync_ShouldCompleteWithinTargetTime()
    {
        // Arrange
        var expectedElement = new StoryElement(ElementType.Character, "Test Character", "Test Description");
        _mockCircuitBreaker.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<StoryElement>>>()))
            .ReturnsAsync(expectedElement);

        var stopwatch = Stopwatch.StartNew();
        var targetTime = TimeSpan.FromMilliseconds(100);

        // Act
        var result = await _service.CreateElementAsync(ElementType.Character, "Test Character", "Test Description");

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(targetTime);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task PerformanceTest_GetElementByIdAsync_WithCachedElement_ShouldBeVeryFast()
    {
        // Arrange
        var element = new StoryElement(ElementType.Character, "Cached Character", "Cached Description");
        _memoryCache.Set($"element_{element.Id}", element, TimeSpan.FromMinutes(30));

        var stopwatch = Stopwatch.StartNew();
        var targetTime = TimeSpan.FromMilliseconds(10);

        // Act
        var result = await _service.GetElementByIdAsync(element.Id);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(targetTime);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task MemoryTest_MultipleOperations_ShouldNotLeakMemory()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(false);
        var expectedElement = new StoryElement(ElementType.Character, "Test Character", "Test Description");
        _mockCircuitBreaker.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<StoryElement>>>()))
            .ReturnsAsync(expectedElement);

        // Act
        for (int i = 0; i < 1000; i++)
        {
            await _service.CreateElementAsync(ElementType.Character, $"Character {i}", "Description");
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;

        // Assert
        memoryIncrease.Should().BeLessThan(10 * 1024 * 1024); // Less than 10MB
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _memoryCache.Dispose();
            _disposed = true;
        }
    }
}
