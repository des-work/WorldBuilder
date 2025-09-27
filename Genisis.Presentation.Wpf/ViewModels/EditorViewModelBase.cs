using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Genisis.Presentation.Wpf.ViewModels;

public abstract partial class EditorViewModelBase<TModel> : ViewModelBase where TModel : INotifyPropertyChanged
{
    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private bool _isSavedMessageVisible;

    public IAsyncRelayCommand SaveCommand { get; }
    protected TModel Model { get; }

    protected EditorViewModelBase(TModel model)
    {
        Model = model;
        Model.PropertyChanged += OnModelPropertyChanged;
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    partial void OnIsDirtyChanged(bool value)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        IsDirty = true;
    }

    private bool CanSave() => IsDirty;

    private async Task SaveAsync()
    {
        if (!IsDirty) return;

        await OnSaveAsync();
        IsDirty = false;

        IsSavedMessageVisible = true;
        await Task.Delay(2000); // Show message for 2 seconds
        IsSavedMessageVisible = false;
    }

    protected abstract Task OnSaveAsync();
}
