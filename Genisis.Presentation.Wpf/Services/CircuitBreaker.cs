using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Circuit breaker implementation for fault tolerance
/// </summary>
public class CircuitBreaker : ICircuitBreaker
{
    private readonly ILogger<CircuitBreaker> _logger;
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private readonly TimeSpan _recoveryTimeout;
    private readonly ConcurrentDictionary<string, CircuitState> _circuitStates = new();
    private readonly ConcurrentDictionary<string, int> _failureCounts = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastFailureTimes = new();

    public CircuitBreaker(
        ILogger<CircuitBreaker> logger,
        int failureThreshold = 5,
        TimeSpan timeout = default,
        TimeSpan recoveryTimeout = default)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _failureThreshold = failureThreshold;
        _timeout = timeout == default ? TimeSpan.FromSeconds(30) : timeout;
        _recoveryTimeout = recoveryTimeout == default ? TimeSpan.FromMinutes(1) : recoveryTimeout;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        var operationId = Guid.NewGuid().ToString();
        var circuitKey = GetCircuitKey(operation);
        
        var currentState = _circuitStates.GetOrAdd(circuitKey, CircuitState.Closed);
        
        if (currentState == CircuitState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTimes.GetValueOrDefault(circuitKey, DateTime.MinValue) > _recoveryTimeout)
            {
                _circuitStates.TryUpdate(circuitKey, CircuitState.HalfOpen, CircuitState.Open);
                _logger.LogInformation("Circuit breaker {CircuitKey} transitioning to HalfOpen state", circuitKey);
            }
            else
            {
                _logger.LogWarning("Circuit breaker {CircuitKey} is open, operation rejected", circuitKey);
                throw new CircuitBreakerOpenException($"Circuit breaker for {circuitKey} is open");
            }
        }

        try
        {
            var result = await operation();
            OnSuccess(circuitKey);
            return result;
        }
        catch (Exception ex)
        {
            OnFailure(circuitKey, ex);
            throw;
        }
    }

    private void OnSuccess(string circuitKey)
    {
        var currentState = _circuitStates.GetOrAdd(circuitKey, CircuitState.Closed);
        
        if (currentState == CircuitState.HalfOpen)
        {
            _circuitStates.TryUpdate(circuitKey, CircuitState.Closed, CircuitState.HalfOpen);
            _logger.LogInformation("Circuit breaker {CircuitKey} transitioning to Closed state", circuitKey);
        }
        
        // Reset failure count on success
        _failureCounts.TryRemove(circuitKey, out _);
    }

    private void OnFailure(string circuitKey, Exception ex)
    {
        var failureCount = _failureCounts.AddOrUpdate(circuitKey, 1, (key, value) => value + 1);
        _lastFailureTimes.AddOrUpdate(circuitKey, DateTime.UtcNow, (key, value) => DateTime.UtcNow);

        _logger.LogWarning(ex, "Circuit breaker {CircuitKey} failure count: {FailureCount}", circuitKey, failureCount);

        if (failureCount >= _failureThreshold)
        {
            _circuitStates.AddOrUpdate(circuitKey, CircuitState.Open, (key, value) => CircuitState.Open);
            _logger.LogWarning("Circuit breaker {CircuitKey} transitioning to Open state after {FailureCount} failures", 
                circuitKey, failureCount);
        }
    }

    private string GetCircuitKey(Func<Task<T>> operation)
    {
        // Use operation type as circuit key
        return operation.Method.DeclaringType?.Name ?? "Unknown";
    }

    /// <summary>
    /// Get current circuit state for a key
    /// </summary>
    /// <param name="circuitKey">Circuit key</param>
    /// <returns>Current circuit state</returns>
    public CircuitState GetCircuitState(string circuitKey)
    {
        return _circuitStates.GetValueOrDefault(circuitKey, CircuitState.Closed);
    }

    /// <summary>
    /// Get failure count for a circuit
    /// </summary>
    /// <param name="circuitKey">Circuit key</param>
    /// <returns>Failure count</returns>
    public int GetFailureCount(string circuitKey)
    {
        return _failureCounts.GetValueOrDefault(circuitKey, 0);
    }

    /// <summary>
    /// Reset circuit breaker for a key
    /// </summary>
    /// <param name="circuitKey">Circuit key</param>
    public void ResetCircuit(string circuitKey)
    {
        _circuitStates.TryRemove(circuitKey, out _);
        _failureCounts.TryRemove(circuitKey, out _);
        _lastFailureTimes.TryRemove(circuitKey, out _);
        _logger.LogInformation("Circuit breaker {CircuitKey} reset", circuitKey);
    }

    /// <summary>
    /// Get all circuit states
    /// </summary>
    /// <returns>Dictionary of circuit states</returns>
    public Dictionary<string, CircuitState> GetAllCircuitStates()
    {
        return _circuitStates.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private enum CircuitState
    {
        Closed,
        Open,
        HalfOpen
    }
}

/// <summary>
/// Exception thrown when circuit breaker is open
/// </summary>
public class CircuitBreakerOpenException : Exception
{
    public CircuitBreakerOpenException(string message) : base(message)
    {
    }

    public CircuitBreakerOpenException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
