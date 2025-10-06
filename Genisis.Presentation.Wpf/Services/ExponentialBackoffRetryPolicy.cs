using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Exponential backoff retry policy implementation
/// </summary>
public class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private readonly ILogger<ExponentialBackoffRetryPolicy> _logger;
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;
    private readonly double _backoffMultiplier;
    private readonly TimeSpan _maxDelay;
    private readonly ConcurrentDictionary<string, int> _operationRetryCounts = new();

    public ExponentialBackoffRetryPolicy(
        ILogger<ExponentialBackoffRetryPolicy> logger,
        int maxRetries = 3,
        TimeSpan initialDelay = default,
        double backoffMultiplier = 2.0,
        TimeSpan maxDelay = default)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxRetries = maxRetries;
        _initialDelay = initialDelay == default ? TimeSpan.FromSeconds(1) : initialDelay;
        _backoffMultiplier = backoffMultiplier;
        _maxDelay = maxDelay == default ? TimeSpan.FromMinutes(5) : maxDelay;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        var operationId = Guid.NewGuid().ToString();
        var delay = _initialDelay;
        
        for (int attempt = 0; attempt <= _maxRetries; attempt++)
        {
            try
            {
                _logger.LogDebug("Executing operation {OperationId}, attempt {Attempt}", operationId, attempt + 1);
                
                var result = await operation();
                
                // Reset retry count on success
                _operationRetryCounts.TryRemove(operationId, out _);
                
                _logger.LogDebug("Operation {OperationId} succeeded on attempt {Attempt}", operationId, attempt + 1);
                return result;
            }
            catch (Exception ex) when (attempt < _maxRetries)
            {
                _operationRetryCounts.AddOrUpdate(operationId, 1, (key, value) => value + 1);
                
                _logger.LogWarning(ex, "Operation {OperationId} failed on attempt {Attempt}, retrying in {Delay}ms", 
                    operationId, attempt + 1, delay.TotalMilliseconds);
                
                await Task.Delay(delay);
                
                // Calculate next delay with exponential backoff
                delay = TimeSpan.FromMilliseconds(
                    Math.Min(delay.TotalMilliseconds * _backoffMultiplier, _maxDelay.TotalMilliseconds));
            }
        }

        // Final attempt - let exception propagate
        _logger.LogError("Operation {OperationId} failed after {MaxRetries} retries", operationId, _maxRetries);
        return await operation();
    }

    /// <summary>
    /// Get retry count for an operation
    /// </summary>
    /// <param name="operationId">Operation identifier</param>
    /// <returns>Number of retries</returns>
    public int GetRetryCount(string operationId)
    {
        return _operationRetryCounts.TryGetValue(operationId, out var count) ? count : 0;
    }

    /// <summary>
    /// Clear retry count for an operation
    /// </summary>
    /// <param name="operationId">Operation identifier</param>
    public void ClearRetryCount(string operationId)
    {
        _operationRetryCounts.TryRemove(operationId, out _);
    }

    /// <summary>
    /// Get total number of operations currently being retried
    /// </summary>
    /// <returns>Number of operations being retried</returns>
    public int GetActiveRetryCount()
    {
        return _operationRetryCounts.Count;
    }
}
