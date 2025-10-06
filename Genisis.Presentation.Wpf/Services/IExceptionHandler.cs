using System.Windows;

namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Interface for handling exceptions in the presentation layer
/// </summary>
public interface IExceptionHandler
{
    /// <summary>
    /// Handles an exception and shows appropriate user feedback
    /// </summary>
    void HandleException(Exception exception, string? context = null);

    /// <summary>
    /// Handles an exception asynchronously
    /// </summary>
    Task HandleExceptionAsync(Exception exception, string? context = null);

    /// <summary>
    /// Shows a user-friendly error message
    /// </summary>
    void ShowError(string message, string? title = null);

    /// <summary>
    /// Shows a user-friendly warning message
    /// </summary>
    void ShowWarning(string message, string? title = null);

    /// <summary>
    /// Shows a user-friendly information message
    /// </summary>
    void ShowInformation(string message, string? title = null);
}
