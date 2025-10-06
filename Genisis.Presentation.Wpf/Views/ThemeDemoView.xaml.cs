using System.Windows;
using System.Windows.Controls;
using Genisis.Presentation.Wpf.Themes;

namespace Genisis.Presentation.Wpf.Views;

/// <summary>
/// Theme demonstration view
/// </summary>
public partial class ThemeDemoView : UserControl
{
    public ThemeDemoView()
    {
        InitializeComponent();
        Loaded += ThemeDemoView_Loaded;
    }

    /// <summary>
    /// Handle view loaded event
    /// </summary>
    private void ThemeDemoView_Loaded(object sender, RoutedEventArgs e)
    {
        // Start demo animation
        var demoAnimation = (System.Windows.Media.Animation.Storyboard)Resources["DemoAnimation"];
        demoAnimation.Begin(DemoElement);
    }
}
