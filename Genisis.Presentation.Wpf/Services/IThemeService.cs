using Genisis.Presentation.Wpf.Themes;

namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Service for managing themes and theme switching
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Currently active theme
    /// </summary>
    IThemeProvider? CurrentTheme { get; }

    /// <summary>
    /// Available themes
    /// </summary>
    IEnumerable<IThemeProvider> AvailableThemes { get; }

    /// <summary>
    /// Whether a theme transition is in progress
    /// </summary>
    bool IsTransitioning { get; }

    /// <summary>
    /// Event raised when theme changes
    /// </summary>
    event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    /// <summary>
    /// Event raised when theme transition starts
    /// </summary>
    event EventHandler<ThemeTransitionEventArgs>? ThemeTransitionStarted;

    /// <summary>
    /// Event raised when theme transition completes
    /// </summary>
    event EventHandler<ThemeTransitionEventArgs>? ThemeTransitionCompleted;

    /// <summary>
    /// Switch to a different theme
    /// </summary>
    Task SwitchThemeAsync(string themeId, bool animate = true);

    /// <summary>
    /// Get a theme by ID
    /// </summary>
    IThemeProvider? GetTheme(string themeId);

    /// <summary>
    /// Initialize the theme service
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Save current theme preference
    /// </summary>
    Task SaveThemePreferenceAsync(string themeId);

    /// <summary>
    /// Load saved theme preference
    /// </summary>
    Task<string?> LoadThemePreferenceAsync();
}
