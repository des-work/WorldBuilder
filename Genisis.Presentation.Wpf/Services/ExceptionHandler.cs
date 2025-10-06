using Genisis.Core.Exceptions;
using Genisis.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Implementation of exception handling for the presentation layer
/// </summary>
public class ExceptionHandler : IExceptionHandler
{
    private readonly ILoggingService _loggingService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(ILoggingService loggingService, IDialogService dialogService, ILogger<ExceptionHandler> logger)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void HandleException(Exception exception, string? context = null)
    {
        try
        {
            LogException(exception, context);
            ShowUserFriendlyMessage(exception, context);
        }
        catch (Exception ex)
        {
            // Fallback to basic logging if our exception handling fails
            _logger.LogCritical(ex, "Critical error in exception handler");
            MessageBox.Show("A critical error occurred. Please restart the application.", "Critical Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task HandleExceptionAsync(Exception exception, string? context = null)
    {
        await Task.Run(() => HandleException(exception, context));
    }

    public void ShowError(string message, string? title = null)
    {
        _dialogService.ShowMessage(message, title ?? "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowWarning(string message, string? title = null)
    {
        _dialogService.ShowMessage(message, title ?? "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public void ShowInformation(string message, string? title = null)
    {
        _dialogService.ShowMessage(message, title ?? "Information", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void LogException(Exception exception, string? context)
    {
        var properties = new Dictionary<string, object>
        {
            ["Context"] = context ?? "Unknown",
            ["ExceptionType"] = exception.GetType().Name,
            ["StackTrace"] = exception.StackTrace ?? string.Empty
        };

        switch (exception)
        {
            case DomainValidationException domainEx:
                _loggingService.LogWarning("Domain validation failed: {Message}", domainEx.Message);
                break;
            case BusinessRuleViolationException businessEx:
                _loggingService.LogWarning("Business rule violated: {RuleName} - {Message}", businessEx.RuleName, businessEx.Message);
                break;
            case EntityNotFoundException entityEx:
                _loggingService.LogWarning("Entity not found: {EntityType} with ID {EntityId}", entityEx.EntityType, entityEx.EntityId);
                break;
            case OperationNotAllowedException operationEx:
                _loggingService.LogWarning("Operation not allowed: {Operation} - {Reason}", operationEx.Operation, operationEx.Reason);
                break;
            case DomainException domainEx:
                _loggingService.LogError(domainEx, "Domain error occurred", properties);
                break;
            default:
                _loggingService.LogError(exception, "Unexpected error occurred", properties);
                break;
        }
    }

    private void ShowUserFriendlyMessage(Exception exception, string? context)
    {
        string message;
        string title;
        MessageBoxImage icon;

        switch (exception)
        {
            case DomainValidationException domainEx:
                message = $"Validation failed:\n{string.Join("\n", domainEx.ValidationErrors)}";
                title = "Validation Error";
                icon = MessageBoxImage.Warning;
                break;
            case BusinessRuleViolationException businessEx:
                message = $"Business rule violation:\n{businessEx.Message}";
                title = "Business Rule Error";
                icon = MessageBoxImage.Warning;
                break;
            case EntityNotFoundException entityEx:
                message = $"The requested {entityEx.EntityType.ToLower()} was not found.";
                title = "Not Found";
                icon = MessageBoxImage.Warning;
                break;
            case OperationNotAllowedException operationEx:
                message = $"Operation not allowed:\n{operationEx.Reason}";
                title = "Operation Not Allowed";
                icon = MessageBoxImage.Warning;
                break;
            case DomainException domainEx:
                message = $"A domain error occurred:\n{domainEx.Message}";
                title = "Domain Error";
                icon = MessageBoxImage.Error;
                break;
            case ArgumentException argEx:
                message = $"Invalid input:\n{argEx.Message}";
                title = "Invalid Input";
                icon = MessageBoxImage.Warning;
                break;
            case InvalidOperationException invalidOpEx:
                message = $"Operation failed:\n{invalidOpEx.Message}";
                title = "Operation Failed";
                icon = MessageBoxImage.Error;
                break;
            default:
                message = "An unexpected error occurred. Please try again or contact support if the problem persists.";
                title = "Unexpected Error";
                icon = MessageBoxImage.Error;
                break;
        }

        if (!string.IsNullOrEmpty(context))
        {
            message = $"{context}\n\n{message}";
        }

        _dialogService.ShowMessage(message, title, MessageBoxButton.OK, icon);
    }
}
