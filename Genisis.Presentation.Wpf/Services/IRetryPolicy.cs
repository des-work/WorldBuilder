namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Retry policy interface for resilient operations
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Execute operation with retry logic
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <returns>Operation result</returns>
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation);
}
