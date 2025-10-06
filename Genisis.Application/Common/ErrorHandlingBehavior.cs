using Genisis.Application.Common;
using Genisis.Core.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genisis.Application.Common;

/// <summary>
/// Error handling behavior for MediatR requests
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ErrorHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ErrorHandlingBehavior<TRequest, TResponse>> _logger;

    public ErrorHandlingBehavior(ILogger<ErrorHandlingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling request {RequestType}: {Message}", typeof(TRequest).Name, ex.Message);
            
            // Handle domain exceptions
            if (ex is DomainException domainEx)
            {
                return HandleDomainException<TResponse>(domainEx);
            }
            
            // Handle validation exceptions
            if (ex is ValidationException validationEx)
            {
                return HandleValidationException<TResponse>(validationEx);
            }
            
            // Handle other exceptions
            return HandleGenericException<TResponse>(ex);
        }
    }

    private static TResponse HandleDomainException<TResponse>(DomainException ex)
    {
        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = typeof(TResponse).GetGenericArguments()[0];
            var failureMethod = typeof(Result<>).MakeGenericType(resultType).GetMethod("Failure", new[] { typeof(string) });
            return (TResponse)failureMethod!.Invoke(null, new object[] { ex.Message })!;
        }
        
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(ex.Message);
        }
        
        throw ex;
    }

    private static TResponse HandleValidationException<TResponse>(ValidationException ex)
    {
        var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
        var errorMessage = string.Join("; ", errors);
        
        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = typeof(TResponse).GetGenericArguments()[0];
            var failureMethod = typeof(Result<>).MakeGenericType(resultType).GetMethod("Failure", new[] { typeof(IEnumerable<string>) });
            return (TResponse)failureMethod!.Invoke(null, new object[] { errors })!;
        }
        
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(errors);
        }
        
        throw ex;
    }

    private static TResponse HandleGenericException<TResponse>(Exception ex)
    {
        var errorMessage = "An unexpected error occurred. Please try again.";
        
        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = typeof(TResponse).GetGenericArguments()[0];
            var failureMethod = typeof(Result<>).MakeGenericType(resultType).GetMethod("Failure", new[] { typeof(string) });
            return (TResponse)failureMethod!.Invoke(null, new object[] { errorMessage })!;
        }
        
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(errorMessage);
        }
        
        throw ex;
    }
}
