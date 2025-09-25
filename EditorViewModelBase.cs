using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Genisis.App.ViewModels;

public abstract class EditorViewModelBase<TModel> : ViewModelBase where TModel : INotifyPropertyChanged
{
    private bool _isDirty;
    public bool IsDirty
    {
        get => _isDirty;
        set { _isDirty = value; OnPropertyChanged(); (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
    }

    private bool _isSavedMessageVisible;
    public bool IsSavedMessageVisible
    {
        get => _isSavedMessageVisible;
        set { _isSavedMessageVisible = value; OnPropertyChanged(); }
    }

    public ICommand SaveCommand { get; }
    protected TModel Model { get; }

    protected EditorViewModelBase(TModel model)
    {
        Model = model;
        Model.PropertyChanged += OnModelPropertyChanged;
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => IsDirty);
    }

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        IsDirty = true;
    }

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