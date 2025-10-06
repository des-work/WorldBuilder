namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Circuit breaker interface for fault tolerance
/// </summary>
public interface ICircuitBreaker
{
    /// <summary>
    /// Execute operation with circuit breaker protection
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <returns>Operation result</returns>
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation);
}
