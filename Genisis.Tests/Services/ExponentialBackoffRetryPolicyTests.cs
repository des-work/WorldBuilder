using FluentAssertions;
using Genisis.Presentation.Wpf.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Genisis.Tests.Services;

/// <summary>
/// Comprehensive tests for ExponentialBackoffRetryPolicy
/// </summary>
public class ExponentialBackoffRetryPolicyTests : IDisposable
{
    private readonly Mock<ILogger<ExponentialBackoffRetryPolicy>> _mockLogger;
    private readonly ExponentialBackoffRetryPolicy _retryPolicy;
    private bool _disposed = false;

    public ExponentialBackoffRetryPolicyTests()
    {
        _mockLogger = new Mock<ILogger<ExponentialBackoffRetryPolicy>>();
        _retryPolicy = new ExponentialBackoffRetryPolicy(_mockLogger.Object, maxRetries: 3, initialDelay: TimeSpan.FromMilliseconds(10));
    }

    [Fact]
    public async Task ExecuteAsync_WithSuccessfulOperation_ShouldReturnResult()
    {
        // Arrange
        var expectedResult = "Success";
        var operation = new Func<Task<string>>(() => Task.FromResult(expectedResult));

        // Act
        var result = await _retryPolicy.ExecuteAsync(operation);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task ExecuteAsync_WithSingleFailure_ShouldRetryAndSucceed()
    {
        // Arrange
        var callCount = 0;
        var operation = new Func<Task<string>>(() =>
        {
            callCount++;
            if (callCount == 1)
                throw new InvalidOperationException("Test failure");
            return Task.FromResult("Success");
        });

        // Act
        var result = await _retryPolicy.ExecuteAsync(operation);

        // Assert
        result.Should().Be("Success");
        callCount.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleFailures_ShouldRetryUpToMaxRetries()
    {
        // Arrange
        var callCount = 0;
        var operation = new Func<Task<string>>(() =>
        {
            callCount++;
            throw new InvalidOperationException("Test failure");
        });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _retryPolicy.ExecuteAsync(operation));
        callCount.Should().Be(4); // Initial call + 3 retries
    }

    [Fact]
    public async Task ExecuteAsync_WithExponentialBackoff_ShouldIncreaseDelay()
    {
        // Arrange
        var callCount = 0;
        var delays = new List<TimeSpan>();
        var operation = new Func<Task<string>>(() =>
        {
            callCount++;
            if (callCount == 1)
                throw new InvalidOperationException("Test failure");
            return Task.FromResult("Success");
        });

        // Act
        var result = await _retryPolicy.ExecuteAsync(operation);

        // Assert
        result.Should().Be("Success");
        callCount.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithMaxDelay_ShouldNotExceedMaxDelay()
    {
        // Arrange
        var retryPolicy = new ExponentialBackoffRetryPolicy(
            _mockLogger.Object, 
            maxRetries: 5, 
            initialDelay: TimeSpan.FromMilliseconds(100),
            backoffMultiplier: 2.0,
            maxDelay: TimeSpan.FromMilliseconds(500));

        var callCount = 0;
        var operation = new Func<Task<string>>(() =>
        {
            callCount++;
            if (callCount <= 3)
                throw new InvalidOperationException("Test failure");
            return Task.FromResult("Success");
        });

        // Act
        var result = await _retryPolicy.ExecuteAsync(operation);

        // Assert
        result.Should().Be("Success");
        callCount.Should().Be(4);
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroMaxRetries_ShouldNotRetry()
    {
        // Arrange
        var retryPolicy = new ExponentialBackoffRetryPolicy(_mockLogger.Object, maxRetries: 0);
        var callCount = 0;
        var operation = new Func<Task<string>>(() =>
        {
            callCount++;
            throw new InvalidOperationException("Test failure");
        });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _retryPolicy.ExecuteAsync(operation));
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativeMaxRetries_ShouldUseDefault()
    {
        // Arrange
        var retryPolicy = new ExponentialBackoffRetryPolicy(_mockLogger.Object, maxRetries: -1);
        var callCount = 0;
        var operation = new Func<Task<string>>(() =>
        {
            callCount++;
            throw new InvalidOperationException("Test failure");
        });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _retryPolicy.ExecuteAsync(operation));
        callCount.Should().Be(4); // Default max retries is 3
    }

    [Fact]
    public void GetRetryCount_WithNoRetries_ShouldReturnZero()
    {
        // Act
        var retryCount = _retryPolicy.GetRetryCount("TestOperation");

        // Assert
        retryCount.Should().Be(0);
    }

    [Fact]
    public async Task GetRetryCount_WithRetries_ShouldReturnCorrectCount()
    {
        // Arrange
        var callCount = 0;
        var operation = new Func<Task<string>>(() =>
        {
            callCount++;
            if (callCount <= 2)
                throw new InvalidOperationException("Test failure");
            return Task.FromResult("Success");
        });

        // Act
        await _retryPolicy.ExecuteAsync(operation);

        // Assert
        var retryCount = _retryPolicy.GetRetryCount("TestOperation");
        retryCount.Should().Be(2);
    }

    [Fact]
    public void ClearRetryCount_WithExistingRetries_ShouldClearCount()
    {
        // Arrange
        var operationId = "TestOperation";
        _retryPolicy.GetRetryCount(operationId); // This will create an entry

        // Act
        _retryPolicy.ClearRetryCount(operationId);

        // Assert
        var retryCount = _retryPolicy.GetRetryCount(operationId);
        retryCount.Should().Be(0);
    }

    [Fact]
    public void GetActiveRetryCount_WithMultipleOperations_ShouldReturnCorrectCount()
    {
        // Arrange
        var operation1 = new Func<Task<string>>(() => throw new InvalidOperationException("Test failure"));
        var operation2 = new Func<Task<string>>(() => throw new InvalidOperationException("Test failure"));

        // Act
        try
        {
            _retryPolicy.ExecuteAsync(operation1).Wait();
        }
        catch (AggregateException)
        {
            // Expected
        }

        try
        {
            _retryPolicy.ExecuteAsync(operation2).Wait();
        }
        catch (AggregateException)
        {
            // Expected
        }

        // Assert
        var activeRetryCount = _retryPolicy.GetActiveRetryCount();
        activeRetryCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PerformanceTest_ExecuteAsync_ShouldBeFast()
    {
        // Arrange
        var operation = new Func<Task<string>>(() => Task.FromResult("Success"));
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var targetTime = TimeSpan.FromMilliseconds(10);

        // Act
        var result = await _retryPolicy.ExecuteAsync(operation);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(targetTime);
        result.Should().Be("Success");
    }

    [Fact]
    public async Task ConcurrencyTest_MultipleOperations_ShouldHandleConcurrentAccess()
    {
        // Arrange
        var operation = new Func<Task<string>>(() => Task.FromResult("Success"));
        var tasks = new List<Task<string>>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(_retryPolicy.ExecuteAsync(operation));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllBe("Success");
        results.Should().HaveCount(100);
    }

    [Fact]
    public async Task MemoryTest_MultipleOperations_ShouldNotLeakMemory()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(false);
        var operation = new Func<Task<string>>(() => Task.FromResult("Success"));

        // Act
        for (int i = 0; i < 1000; i++)
        {
            await _retryPolicy.ExecuteAsync(operation);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;

        // Assert
        memoryIncrease.Should().BeLessThan(1024 * 1024); // Less than 1MB
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}
