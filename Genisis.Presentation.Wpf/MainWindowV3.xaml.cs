using System.Windows;
using System.Windows.Controls;
using Genisis.Presentation.Wpf.Controls;
using Genisis.Presentation.Wpf.Services;
using Genisis.Presentation.Wpf.Themes;

namespace Genisis.Presentation.Wpf;

/// <summary>
/// Main window with integrated theme system and timeline navigation
/// </summary>
public partial class MainWindowV3 : Window
{
    private readonly IThemeService _themeService;
    private bool _isInitialized;

    public MainWindowV3()
    {
        InitializeComponent();
        _themeService = new ThemeService();
        InitializeAsync();
    }

    /// <summary>
    /// Initialize the main window
    /// </summary>
    private async void InitializeAsync()
    {
        try
        {
            // Initialize theme service
            await _themeService.InitializeAsync();

            // Subscribe to theme events
            _themeService.ThemeChanged += OnThemeChanged;
            _themeService.ThemeTransitionStarted += OnThemeTransitionStarted;
            _themeService.ThemeTransitionCompleted += OnThemeTransitionCompleted;

            // Update theme selector
            UpdateThemeSelector();

            // Start enhanced bootscreen animation
            if (_themeService.CurrentTheme != null)
            {
                await EnhancedBootscreenView.StartAnimationAsync(_themeService.CurrentTheme);
            }

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize application: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Update theme selector with current theme
    /// </summary>
    private void UpdateThemeSelector()
    {
        if (_themeService.CurrentTheme == null) return;

        foreach (ComboBoxItem item in ThemeSelector.Items)
        {
            if (item.Tag?.ToString() == _themeService.CurrentTheme.ThemeId)
            {
                ThemeSelector.SelectedItem = item;
                break;
            }
        }
    }

    /// <summary>
    /// Handle theme changed event
    /// </summary>
    private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        // Update timeline navigation theme
        TimelineNavigation.UpdateTheme(e.NewTheme);

        // Update status text
        StatusText.Text = $"Theme: {e.NewTheme.ThemeName}";
    }

    /// <summary>
    /// Handle theme transition started event
    /// </summary>
    private void OnThemeTransitionStarted(object? sender, ThemeTransitionEventArgs e)
    {
        StatusText.Text = "Switching theme...";
    }

    /// <summary>
    /// Handle theme transition completed event
    /// </summary>
    private void OnThemeTransitionCompleted(object? sender, ThemeTransitionEventArgs e)
    {
        StatusText.Text = $"Theme: {e.NewTheme.ThemeName}";
    }

    /// <summary>
    /// Handle enhanced bootscreen completed event
    /// </summary>
    private void EnhancedBootscreenView_BootscreenCompleted(object? sender, EventArgs e)
    {
        // Hide enhanced bootscreen and show main content
        EnhancedBootscreenView.Visibility = Visibility.Collapsed;
        MainContentGrid.Visibility = Visibility.Visible;

        // Start fade in animation for main content
        var fadeInAnimation = (Storyboard)Resources["FadeInAnimation"];
        fadeInAnimation.Begin(MainContentGrid);
    }

    /// <summary>
    /// Handle timeline navigation node clicked event
    /// </summary>
    private void TimelineNavigation_NodeClicked(object? sender, NavigationNodeEventArgs e)
    {
        if (!_isInitialized) return;

        StatusText.Text = $"Navigating to: {e.Node.Label}";

        // Handle navigation based on node ID
        switch (e.Node.Id)
        {
            case "universe":
                NavigateToUniverse();
                break;
            case "story":
                NavigateToStory();
                break;
            case "character":
                NavigateToCharacter();
                break;
            case "chapter":
                NavigateToChapter();
                break;
            case "location":
                NavigateToLocation();
                break;
            case "ai":
                NavigateToAI();
                break;
            case "settings":
                NavigateToSettings();
                break;
        }
    }

    /// <summary>
    /// Handle timeline navigation node hovered event
    /// </summary>
    private void TimelineNavigation_NodeHovered(object? sender, NavigationNodeEventArgs e)
    {
        if (!_isInitialized) return;

        StatusText.Text = e.Node.Tooltip;
    }

    /// <summary>
    /// Handle theme selector changed event
    /// </summary>
    private async void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized) return;

        var selectedItem = ThemeSelector.SelectedItem as ComboBoxItem;
        if (selectedItem?.Tag is string themeId)
        {
            try
            {
                await _themeService.SwitchThemeAsync(themeId, true);
                await _themeService.SaveThemePreferenceAsync(themeId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to switch theme: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Navigate to universe view
    /// </summary>
    private void NavigateToUniverse()
    {
        // TODO: Implement universe navigation
        StatusText.Text = "Universe view - Coming soon";
    }

    /// <summary>
    /// Navigate to story view
    /// </summary>
    private void NavigateToStory()
    {
        // TODO: Implement story navigation
        StatusText.Text = "Story view - Coming soon";
    }

    /// <summary>
    /// Navigate to character view
    /// </summary>
    private void NavigateToCharacter()
    {
        // TODO: Implement character navigation
        StatusText.Text = "Character view - Coming soon";
    }

    /// <summary>
    /// Navigate to chapter view
    /// </summary>
    private void NavigateToChapter()
    {
        // TODO: Implement chapter navigation
        StatusText.Text = "Chapter view - Coming soon";
    }

    /// <summary>
    /// Navigate to location view
    /// </summary>
    private void NavigateToLocation()
    {
        // TODO: Implement location navigation
        StatusText.Text = "Location view - Coming soon";
    }

    /// <summary>
    /// Navigate to AI view
    /// </summary>
    private void NavigateToAI()
    {
        // TODO: Implement AI navigation
        StatusText.Text = "AI Assistant - Coming soon";
    }

    /// <summary>
    /// Navigate to settings view
    /// </summary>
    private void NavigateToSettings()
    {
        // TODO: Implement settings navigation
        StatusText.Text = "Settings - Coming soon";
    }

    /// <summary>
    /// Cleanup resources when window closes
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        // Unsubscribe from events
        _themeService.ThemeChanged -= OnThemeChanged;
        _themeService.ThemeTransitionStarted -= OnThemeTransitionStarted;
        _themeService.ThemeTransitionCompleted -= OnThemeTransitionCompleted;

        // Cleanup enhanced bootscreen
        EnhancedBootscreenView.Cleanup();

        base.OnClosed(e);
    }
}
