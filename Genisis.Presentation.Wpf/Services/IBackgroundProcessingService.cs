namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Background processing service for non-blocking operations
/// </summary>
public interface IBackgroundProcessingService
{
    /// <summary>
    /// Enqueue a task for background processing
    /// </summary>
    /// <typeparam name="T">Parameter type</typeparam>
    /// <param name="task">Task to execute</param>
    /// <param name="parameter">Task parameter</param>
    /// <param name="priority">Task priority</param>
    /// <returns>Task representing the operation</returns>
    Task EnqueueTaskAsync<T>(Func<T, Task> task, T parameter, TaskPriority priority = TaskPriority.Normal);
}
