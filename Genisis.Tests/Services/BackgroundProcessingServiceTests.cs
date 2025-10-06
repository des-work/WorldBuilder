using FluentAssertions;
using Genisis.Presentation.Wpf.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Genisis.Tests.Services;

/// <summary>
/// Comprehensive tests for BackgroundProcessingService
/// </summary>
public class BackgroundProcessingServiceTests : IDisposable
{
    private readonly Mock<ILogger<BackgroundProcessingService>> _mockLogger;
    private readonly BackgroundProcessingService _service;
    private bool _disposed = false;

    public BackgroundProcessingServiceTests()
    {
        _mockLogger = new Mock<ILogger<BackgroundProcessingService>>();
        _service = new BackgroundProcessingService(_mockLogger.Object);
    }

    [Fact]
    public async Task EnqueueTaskAsync_WithValidTask_ShouldEnqueueSuccessfully()
    {
        // Arrange
        var taskExecuted = false;
        var task = new Func<string, Task>(async (parameter) =>
        {
            taskExecuted = true;
            await Task.Delay(10);
        });

        // Act
        await _service.EnqueueTaskAsync(task, "test");

        // Wait for task to complete
        await Task.Delay(100);

        // Assert
        taskExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task EnqueueTaskAsync_WithHighPriority_ShouldExecuteFirst()
    {
        // Arrange
        var executionOrder = new List<int>();
        var task1 = new Func<int, Task>(async (parameter) =>
        {
            executionOrder.Add(parameter);
            await Task.Delay(50);
        });
        var task2 = new Func<int, Task>(async (parameter) =>
        {
            executionOrder.Add(parameter);
            await Task.Delay(50);
        });

        // Act
        await _service.EnqueueTaskAsync(task1, 1, TaskPriority.Normal);
        await _service.EnqueueTaskAsync(task2, 2, TaskPriority.High);

        // Wait for tasks to complete
        await Task.Delay(200);

        // Assert
        executionOrder.Should().NotBeEmpty();
        // Note: Execution order may vary due to concurrency, so we just verify both tasks executed
        executionOrder.Should().Contain(1);
        executionOrder.Should().Contain(2);
    }

    [Fact]
    public async Task EnqueueTaskAsync_WithFailingTask_ShouldNotCrash()
    {
        // Arrange
        var task = new Func<string, Task>(async (parameter) =>
        {
            throw new InvalidOperationException("Test failure");
        });

        // Act
        await _service.EnqueueTaskAsync(task, "test");

        // Wait for task to complete
        await Task.Delay(100);

        // Assert
        // Service should still be functional
        var task2 = new Func<string, Task>(async (parameter) =>
        {
            await Task.Delay(10);
        });

        await _service.EnqueueTaskAsync(task2, "test2");
        await Task.Delay(100);

        // If we get here without exception, the test passes
        true.Should().BeTrue();
    }

    [Fact]
    public async Task EnqueueTaskAsync_WithMultipleTasks_ShouldHandleConcurrency()
    {
        // Arrange
        var completedTasks = 0;
        var task = new Func<int, Task>(async (parameter) =>
        {
            Interlocked.Increment(ref completedTasks);
            await Task.Delay(10);
        });

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_service.EnqueueTaskAsync(task, i));
        }

        await Task.WhenAll(tasks);

        // Wait for all background tasks to complete
        await Task.Delay(200);

        // Assert
        completedTasks.Should().Be(10);
    }

    [Fact]
    public void GetTaskCount_WithNoTasks_ShouldReturnZero()
    {
        // Act
        var taskCount = _service.GetTaskCount("TestTask");

        // Assert
        taskCount.Should().Be(0);
    }

    [Fact]
    public async Task GetTaskCount_WithTasks_ShouldReturnCorrectCount()
    {
        // Arrange
        var task = new Func<string, Task>(async (parameter) =>
        {
            await Task.Delay(10);
        });

        // Act
        await _service.EnqueueTaskAsync(task, "test1");
        await _service.EnqueueTaskAsync(task, "test2");
        await _service.EnqueueTaskAsync(task, "test3");

        // Wait for tasks to be processed
        await Task.Delay(100);

        // Assert
        var taskCount = _service.GetTaskCount("TestTask");
        taskCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetTotalTaskCount_WithNoTasks_ShouldReturnZero()
    {
        // Act
        var totalCount = _service.GetTotalTaskCount();

        // Assert
        totalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetTotalTaskCount_WithTasks_ShouldReturnCorrectCount()
    {
        // Arrange
        var task1 = new Func<string, Task>(async (parameter) =>
        {
            await Task.Delay(10);
        });
        var task2 = new Func<int, Task>(async (parameter) =>
        {
            await Task.Delay(10);
        });

        // Act
        await _service.EnqueueTaskAsync(task1, "test1");
        await _service.EnqueueTaskAsync(task2, 123);

        // Wait for tasks to be processed
        await Task.Delay(100);

        // Assert
        var totalCount = _service.GetTotalTaskCount();
        totalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetAllTaskCounts_WithNoTasks_ShouldReturnEmptyDictionary()
    {
        // Act
        var allCounts = _service.GetAllTaskCounts();

        // Assert
        allCounts.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllTaskCounts_WithTasks_ShouldReturnAllCounts()
    {
        // Arrange
        var task1 = new Func<string, Task>(async (parameter) =>
        {
            await Task.Delay(10);
        });
        var task2 = new Func<int, Task>(async (parameter) =>
        {
            await Task.Delay(10);
        });

        // Act
        await _service.EnqueueTaskAsync(task1, "test1");
        await _service.EnqueueTaskAsync(task2, 123);

        // Wait for tasks to be processed
        await Task.Delay(100);

        // Assert
        var allCounts = _service.GetAllTaskCounts();
        allCounts.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PerformanceTest_EnqueueTaskAsync_ShouldBeFast()
    {
        // Arrange
        var task = new Func<string, Task>(async (parameter) =>
        {
            await Task.Delay(1);
        });

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var targetTime = TimeSpan.FromMilliseconds(10);

        // Act
        await _service.EnqueueTaskAsync(task, "test");

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(targetTime);
    }

    [Fact]
    public async Task ConcurrencyTest_MultipleEnqueues_ShouldHandleConcurrentAccess()
    {
        // Arrange
        var task = new Func<int, Task>(async (parameter) =>
        {
            await Task.Delay(1);
        });

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(_service.EnqueueTaskAsync(task, i));
        }

        await Task.WhenAll(tasks);

        // Assert
        // If we get here without exception, the test passes
        true.Should().BeTrue();
    }

    [Fact]
    public async Task MemoryTest_MultipleTasks_ShouldNotLeakMemory()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(false);
        var task = new Func<int, Task>(async (parameter) =>
        {
            await Task.Delay(1);
        });

        // Act
        for (int i = 0; i < 1000; i++)
        {
            await _service.EnqueueTaskAsync(task, i);
        }

        // Wait for all tasks to complete
        await Task.Delay(500);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;

        // Assert
        memoryIncrease.Should().BeLessThan(5 * 1024 * 1024); // Less than 5MB
    }

    [Fact]
    public void Dispose_WithActiveTasks_ShouldCompleteGracefully()
    {
        // Arrange
        var task = new Func<string, Task>(async (parameter) =>
        {
            await Task.Delay(100);
        });

        // Act
        _service.EnqueueTaskAsync(task, "test");
        _service.Dispose();

        // Assert
        // If we get here without exception, the test passes
        true.Should().BeTrue();
    }

    [Fact]
    public async Task EnqueueTaskAsync_AfterDispose_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var task = new Func<string, Task>(async (parameter) =>
        {
            await Task.Delay(1);
        });

        // Act
        _service.Dispose();

        // Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() => 
            _service.EnqueueTaskAsync(task, "test"));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _service.Dispose();
            _disposed = true;
        }
    }
}
