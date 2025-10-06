using Genisis.Application.Common;
using Genisis.Core.Models;
using Genisis.Presentation.Wpf.Services;
using System.ComponentModel;

namespace Genisis.Presentation.Wpf.ViewModels.Base;

/// <summary>
/// Base class for editor ViewModels with save/load functionality
/// </summary>
/// <typeparam name="T">The entity type being edited</typeparam>
public abstract class EditorViewModelBase<T> : ViewModelBase where T : class
{
    private readonly IDialogService _dialogService;
    private bool _isDirty;
    private bool _isValid = true;
    private string _validationMessage = string.Empty;

    protected EditorViewModelBase(T model, IDialogService dialogService)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    /// <summary>
    /// The entity being edited
    /// </summary>
    public T Model { get; }

    /// <summary>
    /// Indicates if the entity has unsaved changes
    /// </summary>
    public bool IsDirty
    {
        get => _isDirty;
        protected set => SetProperty(ref _isDirty, value);
    }

    /// <summary>
    /// Indicates if the entity is valid
    /// </summary>
    public bool IsValid
    {
        get => _isValid;
        protected set => SetProperty(ref _isValid, value);
    }

    /// <summary>
    /// Validation message for the entity
    /// </summary>
    public string ValidationMessage
    {
        get => _validationMessage;
        protected set => SetProperty(ref _validationMessage, value);
    }

    /// <summary>
    /// Can save the entity
    /// </summary>
    public bool CanSave => IsValid && IsDirty && !IsBusy;

    /// <summary>
    /// Can cancel changes
    /// </summary>
    public bool CanCancel => IsDirty && !IsBusy;

    /// <summary>
    /// Command to save the entity
    /// </summary>
    public IAsyncRelayCommand SaveCommand { get; }

    /// <summary>
    /// Command to cancel changes
    /// </summary>
    public IRelayCommand CancelCommand { get; }

    /// <summary>
    /// Command to reset the entity to its original state
    /// </summary>
    public IRelayCommand ResetCommand { get; }

    protected EditorViewModelBase(T model, IDialogService dialogService)
    {
        Model = model;
        _dialogService = dialogService;

        SaveCommand = new AsyncRelayCommand(SaveAsync, () => CanSave);
        CancelCommand = new RelayCommand(Cancel, () => CanCancel);
        ResetCommand = new RelayCommand(Reset, () => CanCancel);

        // Update command states when properties change
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsValid) || e.PropertyName == nameof(IsDirty) || e.PropertyName == nameof(IsBusy))
        {
            SaveCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();
            ResetCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Saves the entity
    /// </summary>
    protected virtual async Task SaveAsync()
    {
        if (!CanSave) return;

        var result = await ExecuteAsync(async () =>
        {
            await OnSaveAsync();
            IsDirty = false;
        }, "Save");

        if (result)
        {
            _dialogService.ShowMessage("Changes saved successfully.", "Success");
        }
    }

    /// <summary>
    /// Cancels changes and reloads the entity
    /// </summary>
    protected virtual void Cancel()
    {
        if (!CanCancel) return;

        var result = _dialogService.ShowConfirmation(
            "Are you sure you want to cancel? All unsaved changes will be lost.",
            "Cancel Changes");

        if (result)
        {
            Reset();
        }
    }

    /// <summary>
    /// Resets the entity to its original state
    /// </summary>
    protected virtual void Reset()
    {
        if (!CanCancel) return;

        LoadFromModel();
        IsDirty = false;
        Validate();
    }

    /// <summary>
    /// Marks the entity as dirty
    /// </summary>
    protected void MarkAsDirty()
    {
        IsDirty = true;
    }

    /// <summary>
    /// Validates the entity
    /// </summary>
    protected virtual void Validate()
    {
        IsValid = true;
        ValidationMessage = string.Empty;
    }

    /// <summary>
    /// Loads data from the model into the ViewModel
    /// </summary>
    protected abstract void LoadFromModel();

    /// <summary>
    /// Saves the ViewModel data to the model
    /// </summary>
    protected abstract Task OnSaveAsync();

    /// <summary>
    /// Called when a property changes to mark as dirty
    /// </summary>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        
        // Mark as dirty when any property changes (except IsDirty itself)
        if (e.PropertyName != nameof(IsDirty))
        {
            MarkAsDirty();
        }
    }
}
