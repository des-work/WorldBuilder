using Genisis.Presentation.Wpf.Views;
using System.Windows;

namespace Genisis.Presentation.Wpf.Services;

public class DialogService : IDialogService
{
    public bool ShowInputDialog(string prompt, string title, out string result)
    {
        var dialog = new InputDialog(prompt, title);
        var dialogResult = dialog.ShowDialog();
        result = dialog.ResponseText;
        return dialogResult == true;
    }

    public void ShowMessage(string message, string title, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Information)
    {
        MessageBox.Show(message, title, buttons, image);
    }

    public bool ShowConfirmation(string message, string title)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }
}
