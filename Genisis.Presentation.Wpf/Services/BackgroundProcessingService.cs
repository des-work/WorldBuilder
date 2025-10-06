using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Background processing service for non-blocking operations
/// </summary>
public class BackgroundProcessingService : IBackgroundProcessingService, IDisposable
{
    private readonly ILogger<BackgroundProcessingService> _logger;
    private readonly Channel<BackgroundTask> _taskChannel;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly SemaphoreSlim _processingSemaphore;
    private readonly ConcurrentDictionary<string, int> _taskCounts = new();
    private bool _disposed = false;

    public BackgroundProcessingService(ILogger<BackgroundProcessingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _taskChannel = Channel.CreateUnbounded<BackgroundTask>();
        _cancellationTokenSource = new CancellationTokenSource();
        _processingSemaphore = new SemaphoreSlim(Environment.ProcessorCount, Environment.ProcessorCount);
        
        // Start background processing
        _ = Task.Run(ProcessTasksAsync);
    }

    public async Task EnqueueTaskAsync<T>(Func<T, Task> task, T parameter, TaskPriority priority = TaskPriority.Normal)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BackgroundProcessingService));

        var backgroundTask = new BackgroundTask(task, parameter, priority);
        await _taskChannel.Writer.WriteAsync(backgroundTask);
        
        var taskType = task.Method.DeclaringType?.Name ?? "Unknown";
        _taskCounts.AddOrUpdate(taskType, 1, (key, value) => value + 1);
        
        _logger.LogDebug("Enqueued background task {TaskType} with priority {Priority}", taskType, priority);
    }

    private async Task ProcessTasksAsync()
    {
        await foreach (var task in _taskChannel.Reader.ReadAllAsync(_cancellationTokenSource.Token))
        {
            await _processingSemaphore.WaitAsync(_cancellationTokenSource.Token);
            
            _ = Task.Run(async () =>
            {
                try
                {
                    var taskType = task.Operation.Method.DeclaringType?.Name ?? "Unknown";
                    _logger.LogDebug("Processing background task {TaskType} with priority {Priority}", 
                        taskType, task.Priority);
                    
                    await task.ExecuteAsync();
                    
                    _logger.LogDebug("Completed background task {TaskType}", taskType);
                }
                catch (Exception ex)
                {
                    var taskType = task.Operation.Method.DeclaringType?.Name ?? "Unknown";
                    _logger.LogError(ex, "Background task {TaskType} failed", taskType);
                }
                finally
                {
                    _processingSemaphore.Release();
                }
            }, _cancellationTokenSource.Token);
        }
    }

    /// <summary>
    /// Get task count for a specific type
    /// </summary>
    /// <param name="taskType">Task type</param>
    /// <returns>Number of tasks</returns>
    public int GetTaskCount(string taskType)
    {
        return _taskCounts.GetValueOrDefault(taskType, 0);
    }

    /// <summary>
    /// Get total number of tasks in queue
    /// </summary>
    /// <returns>Total task count</returns>
    public int GetTotalTaskCount()
    {
        return _taskCounts.Values.Sum();
    }

    /// <summary>
    /// Get all task counts
    /// </summary>
    /// <returns>Dictionary of task counts</returns>
    public Dictionary<string, int> GetAllTaskCounts()
    {
        return _taskCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _processingSemaphore.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Background task wrapper
/// </summary>
public class BackgroundTask
{
    public Func<object?, Task> Operation { get; }
    public object? Parameter { get; }
    public TaskPriority Priority { get; }
    public DateTime EnqueuedAt { get; }

    public BackgroundTask(Func<object?, Task> operation, object? parameter, TaskPriority priority)
    {
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        Parameter = parameter;
        Priority = priority;
        EnqueuedAt = DateTime.UtcNow;
    }

    public async Task ExecuteAsync()
    {
        await Operation(Parameter);
    }
}

/// <summary>
/// Task priority levels
/// </summary>
public enum TaskPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}
