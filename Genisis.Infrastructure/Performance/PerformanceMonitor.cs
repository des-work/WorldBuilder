using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Genisis.Infrastructure.Performance;

/// <summary>
/// Implementation of performance monitoring
/// </summary>
public class PerformanceMonitor : IPerformanceMonitor
{
    private readonly ConcurrentDictionary<string, PerformanceStats> _stats = new();
    private readonly ILogger<PerformanceMonitor> _logger;

    public PerformanceMonitor(ILogger<PerformanceMonitor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IDisposable StartMonitoring(string operationName)
    {
        return new PerformanceScope(this, operationName);
    }

    public void RecordMetric(string operationName, TimeSpan duration, Dictionary<string, object>? metadata = null)
    {
        var stats = _stats.AddOrUpdate(
            operationName,
            new PerformanceStats
            {
                OperationName = operationName,
                CallCount = 1,
                TotalDuration = duration,
                MinDuration = duration,
                MaxDuration = duration,
                FirstCall = DateTime.UtcNow,
                LastCall = DateTime.UtcNow
            },
            (key, existing) =>
            {
                existing.CallCount++;
                existing.TotalDuration += duration;
                existing.MinDuration = duration < existing.MinDuration ? duration : existing.MinDuration;
                existing.MaxDuration = duration > existing.MaxDuration ? duration : existing.MaxDuration;
                existing.LastCall = DateTime.UtcNow;
                return existing;
            });

        // Log slow operations
        if (duration.TotalMilliseconds > 1000)
        {
            _logger.LogWarning("Slow operation detected: {OperationName} took {Duration}ms", 
                operationName, duration.TotalMilliseconds);
        }

        // Log metadata if provided
        if (metadata != null && metadata.Any())
        {
            _logger.LogDebug("Performance metric recorded: {OperationName} took {Duration}ms with metadata: {@Metadata}", 
                operationName, duration.TotalMilliseconds, metadata);
        }
    }

    public void RecordMetric<T>(string operationName, TimeSpan duration, T result, Dictionary<string, object>? metadata = null)
    {
        var metadataWithResult = metadata ?? new Dictionary<string, object>();
        metadataWithResult["ResultType"] = typeof(T).Name;
        metadataWithResult["HasResult"] = result != null;
        
        RecordMetric(operationName, duration, metadataWithResult);
    }

    public PerformanceStats GetStats(string operationName)
    {
        return _stats.TryGetValue(operationName, out var stats) ? stats : new PerformanceStats { OperationName = operationName };
    }

    public Dictionary<string, PerformanceStats> GetAllStats()
    {
        return _stats.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
