using System.Windows;

namespace Genisis.Presentation.Wpf.Services;

public interface IDialogService
{
    bool ShowInputDialog(string prompt, string title, out string result);
    void ShowMessage(string message, string title, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Information);
    bool ShowConfirmation(string message, string title);
    void ShowError(string message, string title = "Error");
}
