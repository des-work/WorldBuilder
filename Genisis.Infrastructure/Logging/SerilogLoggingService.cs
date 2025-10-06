using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;

namespace Genisis.Infrastructure.Logging;

/// <summary>
/// Serilog-based logging service implementation
/// </summary>
public class SerilogLoggingService : ILoggingService
{
    private readonly ILogger<SerilogLoggingService> _logger;

    public SerilogLoggingService(ILogger<SerilogLoggingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void LogInformation(string message, Dictionary<string, object> properties)
    {
        using (LogContext.PushProperties(properties))
        {
            _logger.LogInformation(message);
        }
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }

    public void LogWarning(string message, Dictionary<string, object> properties)
    {
        using (LogContext.PushProperties(properties))
        {
            _logger.LogWarning(message);
        }
    }

    public void LogError(string message, params object[] args)
    {
        _logger.LogError(message, args);
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        _logger.LogError(exception, message, args);
    }

    public void LogError(string message, Dictionary<string, object> properties)
    {
        using (LogContext.PushProperties(properties))
        {
            _logger.LogError(message);
        }
    }

    public void LogError(Exception exception, string message, Dictionary<string, object> properties)
    {
        using (LogContext.PushProperties(properties))
        {
            _logger.LogError(exception, message);
        }
    }

    public void LogDebug(string message, params object[] args)
    {
        _logger.LogDebug(message, args);
    }

    public void LogDebug(string message, Dictionary<string, object> properties)
    {
        using (LogContext.PushProperties(properties))
        {
            _logger.LogDebug(message);
        }
    }

    public void LogCritical(string message, params object[] args)
    {
        _logger.LogCritical(message, args);
    }

    public void LogCritical(Exception exception, string message, params object[] args)
    {
        _logger.LogCritical(exception, message, args);
    }

    public void LogCritical(string message, Dictionary<string, object> properties)
    {
        using (LogContext.PushProperties(properties))
        {
            _logger.LogCritical(message);
        }
    }

    public void LogCritical(Exception exception, string message, Dictionary<string, object> properties)
    {
        using (LogContext.PushProperties(properties))
        {
            _logger.LogCritical(exception, message);
        }
    }
}
