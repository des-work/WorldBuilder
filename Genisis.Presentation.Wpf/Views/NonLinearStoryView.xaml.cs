using System.Windows;
using System.Windows.Controls;
using Genisis.Presentation.Wpf.Controls;
using Genisis.Presentation.Wpf.ViewModels;

namespace Genisis.Presentation.Wpf.Views;

/// <summary>
/// Non-linear story creation view with multi-context awareness and AI integration
/// </summary>
public partial class NonLinearStoryView : Window
{
    private NonLinearStoryViewModel? _viewModel;
    private EnhancedAIViewModel? _aiViewModel;
    private bool _isInitialized = false;

    public NonLinearStoryView()
    {
        InitializeComponent();
        InitializeAsync();
    }

    /// <summary>
    /// Initialize the view with dependencies
    /// </summary>
    private async void InitializeAsync()
    {
        try
        {
            // TODO: Inject dependencies properly
            // For now, create mock view models
            _viewModel = new NonLinearStoryViewModel(
                null!, // IUniverseRepository
                null!, // IStoryRepository
                null!, // ICharacterRepository
                null!, // IChapterRepository
                null!, // IAiService
                null!  // IDialogService
            );

            _aiViewModel = new EnhancedAIViewModel(
                null!, // IAiService
                null!  // IPromptGenerationService
            );

            // Connect the view models
            _aiViewModel.StoryViewModel = _viewModel;

            // Set data context
            DataContext = _viewModel;

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize view: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handle timeline navigation node clicked event
    /// </summary>
    private void TimelineNavigation_NodeClicked(object? sender, NavigationNodeEventArgs e)
    {
        if (!_isInitialized) return;

        try
        {
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
                    FocusAI();
                    break;
                case "settings":
                    OpenSettings();
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Navigation error: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handle timeline navigation node hovered event
    /// </summary>
    private void TimelineNavigation_NodeHovered(object? sender, NavigationNodeEventArgs e)
    {
        if (!_isInitialized) return;

        // Update status bar with node tooltip
        // This would be implemented based on your status bar implementation
    }

    /// <summary>
    /// Navigate to universe view
    /// </summary>
    private void NavigateToUniverse()
    {
        // TODO: Implement universe navigation
        // This would set the primary context to a universe element
    }

    /// <summary>
    /// Navigate to story view
    /// </summary>
    private void NavigateToStory()
    {
        // TODO: Implement story navigation
        // This would set the primary context to a story element
    }

    /// <summary>
    /// Navigate to character view
    /// </summary>
    private void NavigateToCharacter()
    {
        // TODO: Implement character navigation
        // This would set the primary context to a character element
    }

    /// <summary>
    /// Navigate to chapter view
    /// </summary>
    private void NavigateToChapter()
    {
        // TODO: Implement chapter navigation
        // This would set the primary context to a chapter element
    }

    /// <summary>
    /// Navigate to location view
    /// </summary>
    private void NavigateToLocation()
    {
        // TODO: Implement location navigation
        // This would set the primary context to a location element
    }

    /// <summary>
    /// Focus AI assistant panel
    /// </summary>
    private void FocusAI()
    {
        // TODO: Focus the AI input textbox
        // This would bring focus to the AI chat input
    }

    /// <summary>
    /// Open settings
    /// </summary>
    private void OpenSettings()
    {
        // TODO: Open settings dialog
        // This would open a settings window or panel
    }

    /// <summary>
    /// Handle window closing
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        try
        {
            // Cleanup resources
            _viewModel = null;
            _aiViewModel = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
        }

        base.OnClosed(e);
    }
}
