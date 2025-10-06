using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;

namespace Genisis.Presentation.Wpf.ViewModels.Base;

/// <summary>
/// Base class for all ViewModels with common functionality
/// </summary>
public abstract class ViewModelBase : ObservableObject, INotifyPropertyChanged
{
    private bool _isBusy;
    private string _title = string.Empty;

    /// <summary>
    /// Indicates if the ViewModel is currently performing an operation
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// The title of the ViewModel
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>
    /// Executes an async operation with busy state management
    /// </summary>
    protected async Task ExecuteAsync(Func<Task> operation, string? operationName = null)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await operation();
        }
        catch (Exception ex)
        {
            // Log error and show user-friendly message
            await HandleErrorAsync(ex, operationName);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Executes an async operation with busy state management and returns a result
    /// </summary>
    protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation, string? operationName = null)
    {
        if (IsBusy) return default;

        try
        {
            IsBusy = true;
            return await operation();
        }
        catch (Exception ex)
        {
            // Log error and show user-friendly message
            await HandleErrorAsync(ex, operationName);
            return default;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Handles errors that occur during async operations
    /// </summary>
    protected virtual async Task HandleErrorAsync(Exception ex, string? operationName = null)
    {
        // This can be overridden in derived classes for specific error handling
        // For now, we'll just log the error
        System.Diagnostics.Debug.WriteLine($"Error in {operationName ?? "operation"}: {ex.Message}");
        
        // In a real application, you might want to show a user-friendly error message
        // or log to a proper logging framework
    }

    /// <summary>
    /// Refreshes the ViewModel data
    /// </summary>
    public virtual async Task RefreshAsync()
    {
        await ExecuteAsync(async () =>
        {
            await LoadDataAsync();
        }, "Refresh");
    }

    /// <summary>
    /// Loads data for the ViewModel
    /// </summary>
    protected virtual Task LoadDataAsync()
    {
        return Task.CompletedTask;
    }
}
