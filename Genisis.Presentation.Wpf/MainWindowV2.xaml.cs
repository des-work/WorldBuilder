using Genisis.Presentation.Wpf.ViewModels;
using System.Windows;

namespace Genisis.Presentation.Wpf;

/// <summary>
/// Interaction logic for MainWindowV2.xaml
/// </summary>
public partial class MainWindowV2 : Window
{
    public MainWindowV2()
    {
        InitializeComponent();
    }

    public MainWindowV2(MainViewModelV2 viewModel) : this()
    {
        DataContext = viewModel;
    }
}
