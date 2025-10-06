using Genisis.Infrastructure.Performance;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genisis.Application.Common;

/// <summary>
/// Performance monitoring behavior for MediatR requests
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(IPerformanceMonitor performanceMonitor, ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _performanceMonitor = performanceMonitor ?? throw new ArgumentNullException(nameof(performanceMonitor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var operationName = $"{typeof(TRequest).Name}";
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogDebug("Starting operation: {OperationName}", operationName);
            var response = await next();
            var duration = DateTime.UtcNow - startTime;

            _performanceMonitor.RecordMetric(operationName, duration, response, new Dictionary<string, object>
            {
                ["RequestType"] = typeof(TRequest).Name,
                ["ResponseType"] = typeof(TResponse).Name,
                ["Success"] = true
            });

            _logger.LogDebug("Completed operation: {OperationName} in {Duration}ms", operationName, duration.TotalMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            
            _performanceMonitor.RecordMetric(operationName, duration, new Dictionary<string, object>
            {
                ["RequestType"] = typeof(TRequest).Name,
                ["ResponseType"] = typeof(TResponse).Name,
                ["Success"] = false,
                ["Exception"] = ex.GetType().Name,
                ["ExceptionMessage"] = ex.Message
            });

            _logger.LogError(ex, "Failed operation: {OperationName} after {Duration}ms", operationName, duration.TotalMilliseconds);
            throw;
        }
    }
}
