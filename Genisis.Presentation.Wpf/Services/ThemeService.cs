using System.IO;
using System.Text.Json;
using Genisis.Core.Configuration;
using Genisis.Presentation.Wpf.Themes;
using Microsoft.Extensions.Options;

namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Implementation of theme service for managing themes and theme switching
/// </summary>
public class ThemeService : IThemeService
{
    private readonly ThemeManager _themeManager;
    private readonly string _preferencesPath;
    private IThemeProvider? _currentTheme;
    private readonly string _defaultThemeId;

    public ThemeService() : this(Microsoft.Extensions.Options.Options.Create(new WorldBuilderConfiguration())) { }

    public ThemeService(IOptions<WorldBuilderConfiguration> options)
    {
        _themeManager = new ThemeManager();
        _preferencesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WorldBuilderAI", "theme.json");
        _defaultThemeId = options?.Value?.UI?.DefaultTheme?.ToLowerInvariant() ?? "fantasy";
        
        // Subscribe to theme manager events
        _themeManager.ThemeChanged += OnThemeChanged;
        _themeManager.ThemeTransitionStarted += OnThemeTransitionStarted;
        _themeManager.ThemeTransitionCompleted += OnThemeTransitionCompleted;
    }

    /// <summary>
    /// Currently active theme
    /// </summary>
    public IThemeProvider? CurrentTheme => _currentTheme;

    /// <summary>
    /// Available themes
    /// </summary>
    public IEnumerable<IThemeProvider> AvailableThemes => _themeManager.AvailableThemes;

    /// <summary>
    /// Whether a theme transition is in progress
    /// </summary>
    public bool IsTransitioning => _themeManager.IsTransitioning;

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
    /// Switch to a different theme
    /// </summary>
    public async Task SwitchThemeAsync(string themeId, bool animate = true)
    {
        await _themeManager.SwitchThemeAsync(themeId, animate);
    }

    /// <summary>
    /// Get a theme by ID
    /// </summary>
    public IThemeProvider? GetTheme(string themeId)
    {
        return _themeManager.GetTheme(themeId);
    }

    /// <summary>
    /// Initialize the theme service
    /// </summary>
    public async Task InitializeAsync()
    {
        // Load saved theme preference
        var savedThemeId = await LoadThemePreferenceAsync();
        if (!string.IsNullOrEmpty(savedThemeId))
        {
            var theme = GetTheme(savedThemeId);
            if (theme != null)
            {
                await SwitchThemeAsync(savedThemeId, false);
            }
        }
        else
        {
            // Default to configured theme
            await SwitchThemeAsync(_defaultThemeId, false);
        }
    }

    /// <summary>
    /// Save current theme preference
    /// </summary>
    public async Task SaveThemePreferenceAsync(string themeId)
    {
        try
        {
            var directory = Path.GetDirectoryName(_preferencesPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var preferences = new { ThemeId = themeId };
            var json = JsonSerializer.Serialize(preferences, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_preferencesPath, json);
        }
        catch (Exception ex)
        {
            // Log error but don't throw
            System.Diagnostics.Debug.WriteLine($"Failed to save theme preference: {ex.Message}");
        }
    }

    /// <summary>
    /// Load saved theme preference
    /// </summary>
    public async Task<string?> LoadThemePreferenceAsync()
    {
        try
        {
            if (!File.Exists(_preferencesPath))
                return null;

            var json = await File.ReadAllTextAsync(_preferencesPath);
            var preferences = JsonSerializer.Deserialize<JsonElement>(json);
            
            if (preferences.TryGetProperty("ThemeId", out var themeIdProperty))
            {
                return themeIdProperty.GetString();
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw
            System.Diagnostics.Debug.WriteLine($"Failed to load theme preference: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Handle theme changed event from theme manager
    /// </summary>
    private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        _currentTheme = e.NewTheme;
        ThemeChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Handle theme transition started event from theme manager
    /// </summary>
    private void OnThemeTransitionStarted(object? sender, ThemeTransitionEventArgs e)
    {
        ThemeTransitionStarted?.Invoke(this, e);
    }

    /// <summary>
    /// Handle theme transition completed event from theme manager
    /// </summary>
    private void OnThemeTransitionCompleted(object? sender, ThemeTransitionEventArgs e)
    {
        ThemeTransitionCompleted?.Invoke(this, e);
    }
}
