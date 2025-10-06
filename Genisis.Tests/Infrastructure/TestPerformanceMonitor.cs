using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Genisis.Tests.Infrastructure;

/// <summary>
/// Interface for test performance monitor
/// </summary>
public interface ITestPerformanceMonitor
{
    Task<PerformanceMetrics> StartMonitoringAsync(string testName);
    Task<PerformanceMetrics> StopMonitoringAsync(string testName);
    Task<PerformanceReport> GenerateReportAsync();
    Task<List<PerformanceAlert>> GetPerformanceAlertsAsync();
    Task ResetMetricsAsync();
}

/// <summary>
/// Test performance monitor implementation
/// </summary>
public class TestPerformanceMonitor : ITestPerformanceMonitor
{
    private readonly ILogger<TestPerformanceMonitor> _logger;
    private readonly ConcurrentDictionary<string, PerformanceSession> _sessions = new();
    private readonly ConcurrentDictionary<string, PerformanceMetrics> _metrics = new();
    private readonly List<PerformanceAlert> _alerts = new();
    private readonly object _lock = new();

    public TestPerformanceMonitor(ILogger<TestPerformanceMonitor> logger)
    {
        _logger = logger;
    }

    public async Task<PerformanceMetrics> StartMonitoringAsync(string testName)
    {
        var session = new PerformanceSession
        {
            TestName = testName,
            StartTime = DateTime.UtcNow,
            Process = Process.GetCurrentProcess(),
            InitialMemory = GC.GetTotalMemory(false),
            InitialCpuTime = Process.GetCurrentProcess().TotalProcessorTime
        };

        _sessions[testName] = session;

        _logger.LogDebug("Started performance monitoring for test: {TestName}", testName);
        return new PerformanceMetrics { TestName = testName };
    }

    public async Task<PerformanceMetrics> StopMonitoringAsync(string testName)
    {
        if (!_sessions.TryGetValue(testName, out var session))
        {
            throw new ArgumentException($"No active session found for test: {testName}");
        }

        var endTime = DateTime.UtcNow;
        var endMemory = GC.GetTotalMemory(false);
        var endCpuTime = Process.GetCurrentProcess().TotalProcessorTime;

        var metrics = new PerformanceMetrics
        {
            TestName = testName,
            StartTime = session.StartTime,
            EndTime = endTime,
            Duration = endTime - session.StartTime,
            MemoryUsed = endMemory - session.InitialMemory,
            CpuTimeUsed = endCpuTime - session.InitialCpuTime,
            PeakMemory = GetPeakMemory(session.Process),
            AverageCpuUsage = CalculateAverageCpuUsage(session.Process, session.StartTime, endTime)
        };

        _metrics[testName] = metrics;
        _sessions.TryRemove(testName, out _);

        // Check for performance alerts
        await CheckPerformanceAlertsAsync(metrics);

        _logger.LogDebug("Stopped performance monitoring for test: {TestName}, Duration: {Duration}ms, Memory: {Memory}MB", 
            testName, metrics.Duration.TotalMilliseconds, metrics.MemoryUsed / 1024 / 1024);

        return metrics;
    }

    public async Task<PerformanceReport> GenerateReportAsync()
    {
        var report = new PerformanceReport
        {
            GeneratedAt = DateTime.UtcNow,
            TotalTests = _metrics.Count,
            Metrics = _metrics.Values.ToList()
        };

        if (report.Metrics.Any())
        {
            report.AverageDuration = TimeSpan.FromTicks((long)report.Metrics.Average(m => m.Duration.Ticks));
            report.MaxDuration = report.Metrics.Max(m => m.Duration);
            report.MinDuration = report.Metrics.Min(m => m.Duration);
            
            report.AverageMemoryUsage = report.Metrics.Average(m => m.MemoryUsed);
            report.MaxMemoryUsage = report.Metrics.Max(m => m.MemoryUsed);
            report.MinMemoryUsage = report.Metrics.Min(m => m.MemoryUsed);
            
            report.AverageCpuUsage = report.Metrics.Average(m => m.AverageCpuUsage);
            report.MaxCpuUsage = report.Metrics.Max(m => m.AverageCpuUsage);
            report.MinCpuUsage = report.Metrics.Min(m => m.AverageCpuUsage);
        }

        // Identify slow tests
        report.SlowTests = report.Metrics
            .Where(m => m.Duration > TimeSpan.FromSeconds(5))
            .OrderByDescending(m => m.Duration)
            .ToList();

        // Identify memory-intensive tests
        report.MemoryIntensiveTests = report.Metrics
            .Where(m => m.MemoryUsed > 100 * 1024 * 1024) // 100MB
            .OrderByDescending(m => m.MemoryUsed)
            .ToList();

        // Identify CPU-intensive tests
        report.CpuIntensiveTests = report.Metrics
            .Where(m => m.AverageCpuUsage > 50.0) // 50% CPU
            .OrderByDescending(m => m.AverageCpuUsage)
            .ToList();

        return report;
    }

    public async Task<List<PerformanceAlert>> GetPerformanceAlertsAsync()
    {
        lock (_lock)
        {
            return _alerts.ToList();
        }
    }

    public async Task ResetMetricsAsync()
    {
        _metrics.Clear();
        _sessions.Clear();
        
        lock (_lock)
        {
            _alerts.Clear();
        }

        _logger.LogInformation("Performance metrics reset");
    }

    private long GetPeakMemory(Process process)
    {
        try
        {
            return process.PeakWorkingSet64;
        }
        catch
        {
            return 0;
        }
    }

    private double CalculateAverageCpuUsage(Process process, DateTime startTime, DateTime endTime)
    {
        try
        {
            var duration = endTime - startTime;
            var cpuTimeUsed = process.TotalProcessorTime;
            var totalCpuTime = duration.TotalMilliseconds * Environment.ProcessorCount;
            
            return totalCpuTime > 0 ? (cpuTimeUsed.TotalMilliseconds / totalCpuTime) * 100 : 0;
        }
        catch
        {
            return 0;
        }
    }

    private async Task CheckPerformanceAlertsAsync(PerformanceMetrics metrics)
    {
        var alerts = new List<PerformanceAlert>();

        // Check for slow tests
        if (metrics.Duration > TimeSpan.FromSeconds(10))
        {
            alerts.Add(new PerformanceAlert
            {
                Type = PerformanceAlertType.SlowTest,
                TestName = metrics.TestName,
                Message = $"Test took {metrics.Duration.TotalSeconds:F2} seconds",
                Severity = PerformanceAlertSeverity.Warning,
                Timestamp = DateTime.UtcNow
            });
        }

        // Check for memory-intensive tests
        if (metrics.MemoryUsed > 200 * 1024 * 1024) // 200MB
        {
            alerts.Add(new PerformanceAlert
            {
                Type = PerformanceAlertType.HighMemoryUsage,
                TestName = metrics.TestName,
                Message = $"Test used {metrics.MemoryUsed / 1024 / 1024:F2}MB of memory",
                Severity = PerformanceAlertSeverity.Warning,
                Timestamp = DateTime.UtcNow
            });
        }

        // Check for CPU-intensive tests
        if (metrics.AverageCpuUsage > 80.0) // 80% CPU
        {
            alerts.Add(new PerformanceAlert
            {
                Type = PerformanceAlertType.HighCpuUsage,
                TestName = metrics.TestName,
                Message = $"Test used {metrics.AverageCpuUsage:F2}% CPU on average",
                Severity = PerformanceAlertSeverity.Warning,
                Timestamp = DateTime.UtcNow
            });
        }

        // Check for memory leaks
        if (metrics.MemoryUsed > 500 * 1024 * 1024) // 500MB
        {
            alerts.Add(new PerformanceAlert
            {
                Type = PerformanceAlertType.PotentialMemoryLeak,
                TestName = metrics.TestName,
                Message = $"Test used {metrics.MemoryUsed / 1024 / 1024:F2}MB of memory - potential memory leak",
                Severity = PerformanceAlertSeverity.Critical,
                Timestamp = DateTime.UtcNow
            });
        }

        // Add alerts to the collection
        if (alerts.Any())
        {
            lock (_lock)
            {
                _alerts.AddRange(alerts);
            }

            foreach (var alert in alerts)
            {
                _logger.LogWarning("Performance alert: {AlertType} - {Message}", alert.Type, alert.Message);
            }
        }
    }
}

/// <summary>
/// Performance session
/// </summary>
public class PerformanceSession
{
    public string TestName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public Process Process { get; set; } = null!;
    public long InitialMemory { get; set; }
    public TimeSpan InitialCpuTime { get; set; }
}

/// <summary>
/// Performance metrics
/// </summary>
public class PerformanceMetrics
{
    public string TestName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public long MemoryUsed { get; set; }
    public TimeSpan CpuTimeUsed { get; set; }
    public long PeakMemory { get; set; }
    public double AverageCpuUsage { get; set; }
}

/// <summary>
/// Performance report
/// </summary>
public class PerformanceReport
{
    public DateTime GeneratedAt { get; set; }
    public int TotalTests { get; set; }
    public List<PerformanceMetrics> Metrics { get; set; } = new();
    
    public TimeSpan AverageDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public TimeSpan MinDuration { get; set; }
    
    public double AverageMemoryUsage { get; set; }
    public long MaxMemoryUsage { get; set; }
    public long MinMemoryUsage { get; set; }
    
    public double AverageCpuUsage { get; set; }
    public double MaxCpuUsage { get; set; }
    public double MinCpuUsage { get; set; }
    
    public List<PerformanceMetrics> SlowTests { get; set; } = new();
    public List<PerformanceMetrics> MemoryIntensiveTests { get; set; } = new();
    public List<PerformanceMetrics> CpuIntensiveTests { get; set; } = new();
}

/// <summary>
/// Performance alert
/// </summary>
public class PerformanceAlert
{
    public PerformanceAlertType Type { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public PerformanceAlertSeverity Severity { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Performance alert types
/// </summary>
public enum PerformanceAlertType
{
    SlowTest,
    HighMemoryUsage,
    HighCpuUsage,
    PotentialMemoryLeak,
    ResourceExhaustion
}

/// <summary>
/// Performance alert severity
/// </summary>
public enum PerformanceAlertSeverity
{
    Info,
    Warning,
    Critical
}
