using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Genisis.Presentation.Wpf.Themes;

/// <summary>
/// Manages theme switching and provides theme-related services
/// </summary>
public class ThemeManager : INotifyPropertyChanged
{
    private readonly Dictionary<string, IThemeProvider> _themeProviders = new();
    private IThemeProvider? _currentTheme;
    private bool _isTransitioning;

    public ThemeManager()
    {
        AvailableThemes = new ObservableCollection<IThemeProvider>();
        InitializeDefaultThemes();
    }

    /// <summary>
    /// Currently active theme
    /// </summary>
    public IThemeProvider? CurrentTheme
    {
        get => _currentTheme;
        private set
        {
            if (_currentTheme != value)
            {
                _currentTheme = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentThemeId));
            }
        }
    }

    /// <summary>
    /// Currently active theme ID
    /// </summary>
    public string? CurrentThemeId => CurrentTheme?.ThemeId;

    /// <summary>
    /// Available themes
    /// </summary>
    public ObservableCollection<IThemeProvider> AvailableThemes { get; }

    /// <summary>
    /// Whether a theme transition is in progress
    /// </summary>
    public bool IsTransitioning
    {
        get => _isTransitioning;
        private set
        {
            if (_isTransitioning != value)
            {
                _isTransitioning = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Event raised when theme changes
    /// </summary>
    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    /// <summary>
    /// Event raised when theme transition starts
    /// </summary>
    public event EventHandler<ThemeTransitionEventArgs>? ThemeTransitionStarted;

    /// <summary>
    /// Event raised when theme transition completes
    /// </summary>
    public event EventHandler<ThemeTransitionEventArgs>? ThemeTransitionCompleted;

    /// <summary>
    /// Register a theme provider
    /// </summary>
    public void RegisterTheme(IThemeProvider themeProvider)
    {
        if (themeProvider == null) throw new ArgumentNullException(nameof(themeProvider));

        _themeProviders[themeProvider.ThemeId] = themeProvider;
        AvailableThemes.Add(themeProvider);
        themeProvider.Initialize();
    }

    /// <summary>
    /// Unregister a theme provider
    /// </summary>
    public void UnregisterTheme(string themeId)
    {
        if (_themeProviders.TryGetValue(themeId, out var themeProvider))
        {
            themeProvider.Cleanup();
            _themeProviders.Remove(themeId);
            AvailableThemes.Remove(themeProvider);
        }
    }

    /// <summary>
    /// Switch to a different theme
    /// </summary>
    public async Task SwitchThemeAsync(string themeId, bool animate = true)
    {
        if (!_themeProviders.TryGetValue(themeId, out var newTheme))
        {
            throw new ArgumentException($"Theme '{themeId}' not found", nameof(themeId));
        }

        if (CurrentTheme?.ThemeId == themeId) return;

        var oldTheme = CurrentTheme;
        IsTransitioning = true;

        try
        {
            ThemeTransitionStarted?.Invoke(this, new ThemeTransitionEventArgs(oldTheme, newTheme, animate));

            if (animate && oldTheme != null)
            {
                await AnimateThemeTransitionAsync(oldTheme, newTheme);
            }

            CurrentTheme = newTheme;
            ApplyTheme(newTheme);

            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(oldTheme, newTheme));
            ThemeTransitionCompleted?.Invoke(this, new ThemeTransitionEventArgs(oldTheme, newTheme, animate));
        }
        finally
        {
            IsTransitioning = false;
        }
    }

    /// <summary>
    /// Get a theme provider by ID
    /// </summary>
    public IThemeProvider? GetTheme(string themeId)
    {
        return _themeProviders.TryGetValue(themeId, out var theme) ? theme : null;
    }

    /// <summary>
    /// Initialize default themes
    /// </summary>
    private void InitializeDefaultThemes()
    {
        RegisterTheme(new FantasyThemeProvider());
        RegisterTheme(new SciFiThemeProvider());
        RegisterTheme(new ClassicThemeProvider());
        RegisterTheme(new HorrorThemeProvider());
    }

    /// <summary>
    /// Apply theme to application
    /// </summary>
    private void ApplyTheme(IThemeProvider theme)
    {
        // Clear existing theme resources
        ClearThemeResources();

        // Apply new theme resources
        var resourceDictionary = theme.ResourceDictionary;
        if (resourceDictionary != null)
        {
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }

        // Update application-wide theme properties
        UpdateApplicationTheme(theme);
    }

    /// <summary>
    /// Clear existing theme resources
    /// </summary>
    private void ClearThemeResources()
    {
        var dictionariesToRemove = Application.Current.Resources.MergedDictionaries
            .Where(d => d.Source?.ToString().Contains("Themes") == true)
            .ToList();

        foreach (var dictionary in dictionariesToRemove)
        {
            Application.Current.Resources.MergedDictionaries.Remove(dictionary);
        }
    }

    /// <summary>
    /// Update application-wide theme properties
    /// </summary>
    private void UpdateApplicationTheme(IThemeProvider theme)
    {
        // Update application resources with theme colors
        Application.Current.Resources["PrimaryColor"] = theme.PrimaryColors.Primary;
        Application.Current.Resources["SecondaryColor"] = theme.PrimaryColors.Secondary;
        Application.Current.Resources["AccentColor"] = theme.PrimaryColors.Accent;
        Application.Current.Resources["BackgroundColor"] = theme.PrimaryColors.Background;
        Application.Current.Resources["SurfaceColor"] = theme.PrimaryColors.Surface;
        Application.Current.Resources["TextColor"] = theme.PrimaryColors.Text;
        Application.Current.Resources["TextSecondaryColor"] = theme.PrimaryColors.TextSecondary;
        Application.Current.Resources["BorderColor"] = theme.PrimaryColors.Border;
        Application.Current.Resources["GlowColor"] = theme.PrimaryColors.Glow;
        Application.Current.Resources["ShadowColor"] = theme.PrimaryColors.Shadow;
    }

    /// <summary>
    /// Animate theme transition
    /// </summary>
    private async Task AnimateThemeTransitionAsync(IThemeProvider oldTheme, IThemeProvider newTheme)
    {
        // Create transition animation
        var duration = oldTheme.Animations.DefaultDuration;
        var startTime = DateTime.Now;

        while (DateTime.Now - startTime < duration)
        {
            var progress = (DateTime.Now - startTime).TotalMilliseconds / duration.TotalMilliseconds;
            progress = Math.Min(1.0, progress);

            // Interpolate colors during transition
            InterpolateColors(oldTheme, newTheme, progress);

            await Task.Delay(16); // ~60 FPS
        }

        // Ensure final state
        InterpolateColors(oldTheme, newTheme, 1.0);
    }

    /// <summary>
    /// Interpolate colors between themes
    /// </summary>
    private void InterpolateColors(IThemeProvider oldTheme, IThemeProvider newTheme, double progress)
    {
        var easing = new CubicEase();
        var easedProgress = easing.Ease(progress);

        // Interpolate primary colors
        var primaryColor = InterpolateColor(oldTheme.PrimaryColors.Primary, newTheme.PrimaryColors.Primary, easedProgress);
        var secondaryColor = InterpolateColor(oldTheme.PrimaryColors.Secondary, newTheme.PrimaryColors.Secondary, easedProgress);
        var accentColor = InterpolateColor(oldTheme.PrimaryColors.Accent, newTheme.PrimaryColors.Accent, easedProgress);
        var backgroundColor = InterpolateColor(oldTheme.PrimaryColors.Background, newTheme.PrimaryColors.Background, easedProgress);
        var surfaceColor = InterpolateColor(oldTheme.PrimaryColors.Surface, newTheme.PrimaryColors.Surface, easedProgress);
        var textColor = InterpolateColor(oldTheme.PrimaryColors.Text, newTheme.PrimaryColors.Text, easedProgress);

        // Update application resources
        Application.Current.Resources["PrimaryColor"] = primaryColor;
        Application.Current.Resources["SecondaryColor"] = secondaryColor;
        Application.Current.Resources["AccentColor"] = accentColor;
        Application.Current.Resources["BackgroundColor"] = backgroundColor;
        Application.Current.Resources["SurfaceColor"] = surfaceColor;
        Application.Current.Resources["TextColor"] = textColor;
    }

    /// <summary>
    /// Interpolate between two colors
    /// </summary>
    private static Color InterpolateColor(Color start, Color end, double progress)
    {
        var r = (byte)(start.R + (end.R - start.R) * progress);
        var g = (byte)(start.G + (end.G - start.G) * progress);
        var b = (byte)(start.B + (end.B - start.B) * progress);
        var a = (byte)(start.A + (end.A - start.A) * progress);

        return Color.FromArgb(a, r, g, b);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Event args for theme change events
/// </summary>
public class ThemeChangedEventArgs : EventArgs
{
    public IThemeProvider? OldTheme { get; }
    public IThemeProvider NewTheme { get; }

    public ThemeChangedEventArgs(IThemeProvider? oldTheme, IThemeProvider newTheme)
    {
        OldTheme = oldTheme;
        NewTheme = newTheme;
    }
}

/// <summary>
/// Event args for theme transition events
/// </summary>
public class ThemeTransitionEventArgs : EventArgs
{
    public IThemeProvider? OldTheme { get; }
    public IThemeProvider NewTheme { get; }
    public bool Animated { get; }

    public ThemeTransitionEventArgs(IThemeProvider? oldTheme, IThemeProvider newTheme, bool animated)
    {
        OldTheme = oldTheme;
        NewTheme = newTheme;
        Animated = animated;
    }
}
