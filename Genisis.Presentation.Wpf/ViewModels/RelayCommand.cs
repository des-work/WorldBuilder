using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

// Re-export AsyncRelayCommand and RelayCommand for backward compatibility
// New code should use CommunityToolkit.Mvvm.Input.AsyncRelayCommand directly

namespace Genisis.Presentation.Wpf.ViewModels;

public class RelayCommand : RelayCommand<object?>
{
    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        : base(execute, canExecute)
    {
    }
}
