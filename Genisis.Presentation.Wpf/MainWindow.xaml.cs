using Genisis.Core.Models;
using Genisis.Presentation.Wpf.ViewModels;
using System.Windows;
using System.Windows.Controls;

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

    private async void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
    {
        if (sender is TreeViewItem treeViewItem && DataContext is MainViewModel viewModel)
        {
            var dataItem = treeViewItem.DataContext;
            
            switch (dataItem)
            {
                case Universe universe:
                    // Load stories and characters when universe is expanded
                    await viewModel.LoadUniverseChildrenAsync(universe);
                    break;
                    
                case Story story:
                    // Load chapters when story is expanded
                    await viewModel.LoadStoryChildrenAsync(story);
                    break;
                    
                case CharacterFolderViewModel characterFolder:
                    // Load characters when character folder is expanded
                    await viewModel.LoadCharacterFolderChildrenAsync(characterFolder);
                    break;
            }
        }
    }
}
