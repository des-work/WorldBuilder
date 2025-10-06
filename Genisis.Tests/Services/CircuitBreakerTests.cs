using FluentAssertions;
using Genisis.Presentation.Wpf.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Genisis.Tests.Services;

/// <summary>
/// Comprehensive tests for CircuitBreaker
/// </summary>
public class CircuitBreakerTests : IDisposable
{
    private readonly Mock<ILogger<CircuitBreaker>> _mockLogger;
    private readonly CircuitBreaker _circuitBreaker;
    private bool _disposed = false;

    public CircuitBreakerTests()
    {
        _mockLogger = new Mock<ILogger<CircuitBreaker>>();
        _circuitBreaker = new CircuitBreaker(_mockLogger.Object, failureThreshold: 3, recoveryTimeout: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ExecuteAsync_WithSuccessfulOperation_ShouldReturnResult()
    {
        // Arrange
        var expectedResult = "Success";
        var operation = new Func<Task<string>>(() => Task.FromResult(expectedResult));

        // Act
        var result = await _circuitBreaker.ExecuteAsync(operation);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task ExecuteAsync_WithSingleFailure_ShouldNotOpenCircuit()
    {
        // Arrange
        var operation = new Func<Task<string>>(() => throw new InvalidOperationException("Test failure"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _circuitBreaker.ExecuteAsync(operation));
        
        // Circuit should still be closed
        var circuitState = _circuitBreaker.GetCircuitState("TestOperation");
        circuitState.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleFailures_ShouldOpenCircuit()
    {
        // Arrange
        var operation = new Func<Task<string>>(() => throw new InvalidOperationException("Test failure"));

        // Act
        for (int i = 0; i < 3; i++)
        {
            try
            {
                await _circuitBreaker.ExecuteAsync(operation);
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        // Assert
        var circuitState = _circuitBreaker.GetCircuitState("TestOperation");
        circuitState.Should().Be(CircuitState.Open);
    }

    [Fact]
    public async Task ExecuteAsync_WithOpenCircuit_ShouldThrowCircuitBreakerOpenException()
    {
        // Arrange
        var operation = new Func<Task<string>>(() => throw new InvalidOperationException("Test failure"));

        // Open the circuit
        for (int i = 0; i < 3; i++)
        {
            try
            {
                await _circuitBreaker.ExecuteAsync(operation);
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        // Act & Assert
        await Assert.ThrowsAsync<CircuitBreakerOpenException>(() => _circuitBreaker.ExecuteAsync(operation));
    }

    [Fact]
    public async Task ExecuteAsync_WithRecoveryTimeout_ShouldTransitionToHalfOpen()
    {
        // Arrange
        var operation = new Func<Task<string>>(() => throw new InvalidOperationException("Test failure"));

        // Open the circuit
        for (int i = 0; i < 3; i++)
        {
            try
            {
                await _circuitBreaker.ExecuteAsync(operation);
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        // Wait for recovery timeout
        await Task.Delay(1100);

        // Act
        try
        {
            await _circuitBreaker.ExecuteAsync(operation);
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // Assert
        var circuitState = _circuitBreaker.GetCircuitState("TestOperation");
        circuitState.Should().Be(CircuitState.HalfOpen);
    }

    [Fact]
    public async Task ExecuteAsync_WithHalfOpenSuccess_ShouldTransitionToClosed()
    {
        // Arrange
        var failingOperation = new Func<Task<string>>(() => throw new InvalidOperationException("Test failure"));
        var succeedingOperation = new Func<Task<string>>(() => Task.FromResult("Success"));

        // Open the circuit
        for (int i = 0; i < 3; i++)
        {
            try
            {
                await _circuitBreaker.ExecuteAsync(failingOperation);
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        // Wait for recovery timeout
        await Task.Delay(1100);

        // Act
        var result = await _circuitBreaker.ExecuteAsync(succeedingOperation);

        // Assert
        result.Should().Be("Success");
        var circuitState = _circuitBreaker.GetCircuitState("TestOperation");
        circuitState.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task ExecuteAsync_WithHalfOpenFailure_ShouldTransitionToOpen()
    {
        // Arrange
        var operation = new Func<Task<string>>(() => throw new InvalidOperationException("Test failure"));

        // Open the circuit
        for (int i = 0; i < 3; i++)
        {
            try
            {
                await _circuitBreaker.ExecuteAsync(operation);
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        // Wait for recovery timeout
        await Task.Delay(1100);

        // Act
        try
        {
            await _circuitBreaker.ExecuteAsync(operation);
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // Assert
        var circuitState = _circuitBreaker.GetCircuitState("TestOperation");
        circuitState.Should().Be(CircuitState.Open);
    }

    [Fact]
    public void GetFailureCount_WithNoFailures_ShouldReturnZero()
    {
        // Act
        var failureCount = _circuitBreaker.GetFailureCount("TestOperation");

        // Assert
        failureCount.Should().Be(0);
    }

    [Fact]
    public async Task GetFailureCount_WithFailures_ShouldReturnCorrectCount()
    {
        // Arrange
        var operation = new Func<Task<string>>(() => throw new InvalidOperationException("Test failure"));

        // Act
        for (int i = 0; i < 2; i++)
        {
            try
            {
                await _circuitBreaker.ExecuteAsync(operation);
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        // Assert
        var failureCount = _circuitBreaker.GetFailureCount("TestOperation");
        failureCount.Should().Be(2);
    }

    [Fact]
    public void ResetCircuit_WithOpenCircuit_ShouldCloseCircuit()
    {
        // Arrange
        var operation = new Func<Task<string>>(() => throw new InvalidOperationException("Test failure"));

        // Open the circuit
        for (int i = 0; i < 3; i++)
        {
            try
            {
                _circuitBreaker.ExecuteAsync(operation).Wait();
            }
            catch (AggregateException)
            {
                // Expected
            }
        }

        // Act
        _circuitBreaker.ResetCircuit("TestOperation");

        // Assert
        var circuitState = _circuitBreaker.GetCircuitState("TestOperation");
        circuitState.Should().Be(CircuitState.Closed);
        
        var failureCount = _circuitBreaker.GetFailureCount("TestOperation");
        failureCount.Should().Be(0);
    }

    [Fact]
    public void GetAllCircuitStates_WithMultipleCircuits_ShouldReturnAllStates()
    {
        // Arrange
        var operation1 = new Func<Task<string>>(() => throw new InvalidOperationException("Test failure"));
        var operation2 = new Func<Task<string>>(() => Task.FromResult("Success"));

        // Open circuit 1
        for (int i = 0; i < 3; i++)
        {
            try
            {
                _circuitBreaker.ExecuteAsync(operation1).Wait();
            }
            catch (AggregateException)
            {
                // Expected
            }
        }

        // Use circuit 2 successfully
        _circuitBreaker.ExecuteAsync(operation2).Wait();

        // Act
        var allStates = _circuitBreaker.GetAllCircuitStates();

        // Assert
        allStates.Should().NotBeEmpty();
        allStates.Should().ContainKey("TestOperation");
    }

    [Fact]
    public async Task PerformanceTest_ExecuteAsync_ShouldBeFast()
    {
        // Arrange
        var operation = new Func<Task<string>>(() => Task.FromResult("Success"));
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var targetTime = TimeSpan.FromMilliseconds(10);

        // Act
        var result = await _circuitBreaker.ExecuteAsync(operation);

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
            tasks.Add(_circuitBreaker.ExecuteAsync(operation));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllBe("Success");
        results.Should().HaveCount(100);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}
