using Genisis.Presentation.Wpf.ViewModels;
using System.Windows;

namespace Genisis.Presentation.Wpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MainTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.SelectedItem = e.NewValue;
        }
    }
}
