namespace Genisis.Infrastructure.Logging;

/// <summary>
/// Interface for structured logging services
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Logs an information message
    /// </summary>
    void LogInformation(string message, params object[] args);

    /// <summary>
    /// Logs an information message with structured data
    /// </summary>
    void LogInformation(string message, Dictionary<string, object> properties);

    /// <summary>
    /// Logs a warning message
    /// </summary>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Logs a warning message with structured data
    /// </summary>
    void LogWarning(string message, Dictionary<string, object> properties);

    /// <summary>
    /// Logs an error message
    /// </summary>
    void LogError(string message, params object[] args);

    /// <summary>
    /// Logs an error message with exception
    /// </summary>
    void LogError(Exception exception, string message, params object[] args);

    /// <summary>
    /// Logs an error message with structured data
    /// </summary>
    void LogError(string message, Dictionary<string, object> properties);

    /// <summary>
    /// Logs an error message with exception and structured data
    /// </summary>
    void LogError(Exception exception, string message, Dictionary<string, object> properties);

    /// <summary>
    /// Logs a debug message
    /// </summary>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// Logs a debug message with structured data
    /// </summary>
    void LogDebug(string message, Dictionary<string, object> properties);

    /// <summary>
    /// Logs a critical message
    /// </summary>
    void LogCritical(string message, params object[] args);

    /// <summary>
    /// Logs a critical message with exception
    /// </summary>
    void LogCritical(Exception exception, string message, params object[] args);

    /// <summary>
    /// Logs a critical message with structured data
    /// </summary>
    void LogCritical(string message, Dictionary<string, object> properties);

    /// <summary>
    /// Logs a critical message with exception and structured data
    /// </summary>
    void LogCritical(Exception exception, string message, Dictionary<string, object> properties);
}
