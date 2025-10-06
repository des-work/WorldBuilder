using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Genisis.Presentation.Wpf.Bootscreen;
using Genisis.Presentation.Wpf.Bootscreen.Elements;
using Genisis.Presentation.Wpf.Themes;

namespace Genisis.Presentation.Wpf.Views;

/// <summary>
/// Enhanced bootscreen view with modular, composable elements
/// </summary>
public partial class EnhancedBootscreenView : UserControl
{
    private readonly BootscreenComposer _composer;
    private IThemeProvider? _currentTheme;

    public event EventHandler? BootscreenCompleted;

    public EnhancedBootscreenView()
    {
        InitializeComponent();
        _composer = new BootscreenComposer();
        InitializeBootscreenElements();
        SubscribeToComposerEvents();
    }

    /// <summary>
    /// Start the enhanced bootscreen animation
    /// </summary>
    public async Task StartAnimationAsync(IThemeProvider theme)
    {
        _currentTheme = theme;
        UpdateThemeColors();
        
        // Start progress animations
        StartProgressAnimations();
        
        // Start bootscreen composition
        await _composer.StartBootscreenAsync(theme);
    }

    /// <summary>
    /// Initialize bootscreen elements
    /// </summary>
    private void InitializeBootscreenElements()
    {
        // Add star field first (background)
        var starField = new StarFieldElement();
        _composer.AddElement(starField);
        MainCanvas.Children.Add(starField.VisualElement);

        // Add cosmic explosion
        var explosion = new CosmicExplosionElement();
        _composer.AddElement(explosion);
        MainCanvas.Children.Add(explosion.VisualElement);

        // Add galaxy formation
        var galaxy = new GalaxyFormationElement();
        _composer.AddElement(galaxy);
        MainCanvas.Children.Add(galaxy.VisualElement);

        // Add title reveal last (foreground)
        var titleReveal = new TitleRevealElement();
        _composer.AddElement(titleReveal);
        MainCanvas.Children.Add(titleReveal.VisualElement);
    }

    /// <summary>
    /// Subscribe to composer events
    /// </summary>
    private void SubscribeToComposerEvents()
    {
        _composer.BootscreenStarted += OnBootscreenStarted;
        _composer.BootscreenCompleted += OnBootscreenCompleted;
        _composer.ProgressChanged += OnProgressChanged;
    }

    /// <summary>
    /// Handle bootscreen started event
    /// </summary>
    private void OnBootscreenStarted(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            ProgressText.Text = "Creating universe...";
        });
    }

    /// <summary>
    /// Handle bootscreen completed event
    /// </summary>
    private void OnBootscreenCompleted(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            ProgressText.Text = "Ready";
            BootscreenCompleted?.Invoke(this, EventArgs.Empty);
        });
    }

    /// <summary>
    /// Handle progress changed event
    /// </summary>
    private void OnProgressChanged(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            var progress = _composer.AnimationProgress;
            var progressText = GetProgressText(progress);
            ProgressText.Text = progressText;
        });
    }

    /// <summary>
    /// Get progress text based on completion percentage
    /// </summary>
    private string GetProgressText(double progress)
    {
        return progress switch
        {
            < 0.25 => "Initializing cosmic forces...",
            < 0.5 => "Forming galaxies...",
            < 0.75 => "Creating stars...",
            < 1.0 => "Revealing title...",
            _ => "Ready"
        };
    }

    /// <summary>
    /// Start progress animations
    /// </summary>
    private void StartProgressAnimations()
    {
        // Start progress bar animation
        var progressAnimation = (Storyboard)Resources["ProgressAnimation"];
        progressAnimation.Begin(ProgressBar);

        // Start loading text animation
        var loadingAnimation = (Storyboard)Resources["LoadingTextAnimation"];
        loadingAnimation.Begin(LoadingText);
    }

    /// <summary>
    /// Update theme colors
    /// </summary>
    private void UpdateThemeColors()
    {
        if (_currentTheme == null) return;

        // Update progress bar color
        ProgressBar.Foreground = new SolidColorBrush(_currentTheme.PrimaryColors.Glow);
        ProgressBar.Background = new SolidColorBrush(_currentTheme.PrimaryColors.Surface);

        // Update text colors
        LoadingText.Foreground = new SolidColorBrush(_currentTheme.PrimaryColors.TextSecondary);
        ProgressText.Foreground = new SolidColorBrush(_currentTheme.PrimaryColors.TextSecondary);

        // Update background
        Background = new SolidColorBrush(_currentTheme.PrimaryColors.Background);
    }

    /// <summary>
    /// Cleanup resources
    /// </summary>
    public void Cleanup()
    {
        // Stop animations
        var progressAnimation = (Storyboard)Resources["ProgressAnimation"];
        progressAnimation.Stop();
        
        var loadingAnimation = (Storyboard)Resources["LoadingTextAnimation"];
        loadingAnimation.Stop();

        // Cleanup composer
        _composer.Cleanup();

        // Clear canvas
        MainCanvas.Children.Clear();
        OverlayCanvas.Children.Clear();
    }
}
