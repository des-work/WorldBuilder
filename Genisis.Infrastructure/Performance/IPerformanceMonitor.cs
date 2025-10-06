namespace Genisis.Infrastructure.Performance;

/// <summary>
/// Interface for performance monitoring
/// </summary>
public interface IPerformanceMonitor
{
    /// <summary>
    /// Starts monitoring a performance metric
    /// </summary>
    IDisposable StartMonitoring(string operationName);

    /// <summary>
    /// Records a performance metric
    /// </summary>
    void RecordMetric(string operationName, TimeSpan duration, Dictionary<string, object>? metadata = null);

    /// <summary>
    /// Records a performance metric with result
    /// </summary>
    void RecordMetric<T>(string operationName, TimeSpan duration, T result, Dictionary<string, object>? metadata = null);

    /// <summary>
    /// Gets performance statistics for an operation
    /// </summary>
    PerformanceStats GetStats(string operationName);

    /// <summary>
    /// Gets all performance statistics
    /// </summary>
    Dictionary<string, PerformanceStats> GetAllStats();
}

/// <summary>
/// Performance statistics for an operation
/// </summary>
public class PerformanceStats
{
    public string OperationName { get; set; } = string.Empty;
    public int CallCount { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan AverageDuration => CallCount > 0 ? TimeSpan.FromTicks(TotalDuration.Ticks / CallCount) : TimeSpan.Zero;
    public TimeSpan MinDuration { get; set; } = TimeSpan.MaxValue;
    public TimeSpan MaxDuration { get; set; } = TimeSpan.Zero;
    public DateTime LastCall { get; set; }
    public DateTime FirstCall { get; set; }
}

/// <summary>
/// Performance monitoring scope
/// </summary>
public class PerformanceScope : IDisposable
{
    private readonly IPerformanceMonitor _monitor;
    private readonly string _operationName;
    private readonly DateTime _startTime;
    private readonly Dictionary<string, object>? _metadata;

    public PerformanceScope(IPerformanceMonitor monitor, string operationName, Dictionary<string, object>? metadata = null)
    {
        _monitor = monitor;
        _operationName = operationName;
        _startTime = DateTime.UtcNow;
        _metadata = metadata;
    }

    public void Dispose()
    {
        var duration = DateTime.UtcNow - _startTime;
        _monitor.RecordMetric(_operationName, duration, _metadata);
    }
}
