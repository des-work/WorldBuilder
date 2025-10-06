using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Genisis.Presentation.Wpf.Themes;

namespace Genisis.Presentation.Wpf.Bootscreen;

/// <summary>
/// Composes multiple bootscreen elements into a cohesive experience
/// </summary>
public class BootscreenComposer : INotifyPropertyChanged
{
    private readonly ObservableCollection<IBootscreenElement> _elements = new();
    private readonly Dictionary<string, IBootscreenElement> _elementRegistry = new();
    private IThemeProvider? _currentTheme;
    private bool _isAnimating;
    private int _completedElements;

    public BootscreenComposer()
    {
        Elements = new ReadOnlyObservableCollection<IBootscreenElement>(_elements);
    }

    /// <summary>
    /// Bootscreen elements
    /// </summary>
    public ReadOnlyObservableCollection<IBootscreenElement> Elements { get; }

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
            }
        }
    }

    /// <summary>
    /// Whether the bootscreen is currently animating
    /// </summary>
    public bool IsAnimating
    {
        get => _isAnimating;
        private set
        {
            if (_isAnimating != value)
            {
                _isAnimating = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Progress of the bootscreen animation (0-1)
    /// </summary>
    public double AnimationProgress
    {
        get => _elements.Count > 0 ? (double)_completedElements / _elements.Count : 0;
    }

    /// <summary>
    /// Event raised when bootscreen animation completes
    /// </summary>
    public event EventHandler? BootscreenCompleted;

    /// <summary>
    /// Event raised when bootscreen animation starts
    /// </summary>
    public event EventHandler? BootscreenStarted;

    /// <summary>
    /// Event raised when bootscreen progress changes
    /// </summary>
    public event EventHandler? ProgressChanged;

    /// <summary>
    /// Add an element to the bootscreen composition
    /// </summary>
    public void AddElement(IBootscreenElement element)
    {
        if (element == null) throw new ArgumentNullException(nameof(element));
        if (_elementRegistry.ContainsKey(element.ElementId))
            throw new InvalidOperationException($"Element with ID '{element.ElementId}' already exists");

        _elementRegistry[element.ElementId] = element;
        _elements.Add(element);

        // Subscribe to element events
        element.AnimationCompleted += OnElementAnimationCompleted;
        element.AnimationStarted += OnElementAnimationStarted;
    }

    /// <summary>
    /// Remove an element from the bootscreen composition
    /// </summary>
    public void RemoveElement(string elementId)
    {
        if (_elementRegistry.TryGetValue(elementId, out var element))
        {
            // Unsubscribe from events
            element.AnimationCompleted -= OnElementAnimationCompleted;
            element.AnimationStarted -= OnElementAnimationStarted;

            // Cleanup element
            element.Cleanup();

            // Remove from collections
            _elementRegistry.Remove(elementId);
            _elements.Remove(element);
        }
    }

    /// <summary>
    /// Get an element by ID
    /// </summary>
    public IBootscreenElement? GetElement(string elementId)
    {
        return _elementRegistry.TryGetValue(elementId, out var element) ? element : null;
    }

    /// <summary>
    /// Start the bootscreen animation sequence
    /// </summary>
    public async Task StartBootscreenAsync(IThemeProvider theme)
    {
        if (IsAnimating) return;

        CurrentTheme = theme;
        IsAnimating = true;
        _completedElements = 0;

        BootscreenStarted?.Invoke(this, EventArgs.Empty);

        try
        {
            // Initialize all elements
            foreach (var element in _elements)
            {
                element.Initialize();
                element.UpdateTheme(theme);
            }

            // Start elements in sequence with calculated delays
            var totalDuration = theme.Bootscreen.TotalDuration;
            var elementDelay = TimeSpan.FromMilliseconds(totalDuration.TotalMilliseconds / _elements.Count);

            for (int i = 0; i < _elements.Count; i++)
            {
                var element = _elements[i];
                var delay = TimeSpan.FromMilliseconds(elementDelay.TotalMilliseconds * i);
                
                // Start element animation (fire and forget)
                _ = Task.Run(async () => await element.StartAnimationAsync(theme, delay));
            }

            // Wait for all elements to complete
            await WaitForCompletionAsync();
        }
        finally
        {
            IsAnimating = false;
        }
    }

    /// <summary>
    /// Stop the bootscreen animation
    /// </summary>
    public void StopBootscreen()
    {
        if (!IsAnimating) return;

        foreach (var element in _elements)
        {
            element.StopAnimation();
        }

        IsAnimating = false;
    }

    /// <summary>
    /// Update theme for all elements
    /// </summary>
    public void UpdateTheme(IThemeProvider theme)
    {
        CurrentTheme = theme;
        foreach (var element in _elements)
        {
            element.UpdateTheme(theme);
        }
    }

    /// <summary>
    /// Cleanup all elements
    /// </summary>
    public void Cleanup()
    {
        StopBootscreen();

        foreach (var element in _elements.ToList())
        {
            RemoveElement(element.ElementId);
        }
    }

    /// <summary>
    /// Wait for all elements to complete their animations
    /// </summary>
    private async Task WaitForCompletionAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        var timeout = TimeSpan.FromSeconds(30); // Maximum wait time

        EventHandler? onCompleted = null;
        onCompleted = (sender, e) =>
        {
            if (_completedElements >= _elements.Count)
            {
                tcs.SetResult(true);
                BootscreenCompleted?.Invoke(this, EventArgs.Empty);
            }
        };

        BootscreenCompleted += onCompleted;

        try
        {
            // Wait for completion or timeout
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeout));
            if (completedTask == tcs.Task)
            {
                await tcs.Task;
            }
            else
            {
                // Timeout occurred
                System.Diagnostics.Debug.WriteLine("Bootscreen animation timed out");
            }
        }
        finally
        {
            BootscreenCompleted -= onCompleted;
        }
    }

    /// <summary>
    /// Handle element animation completed event
    /// </summary>
    private void OnElementAnimationCompleted(object? sender, BootscreenElementEventArgs e)
    {
        _completedElements++;
        ProgressChanged?.Invoke(this, EventArgs.Empty);
        OnPropertyChanged(nameof(AnimationProgress));
    }

    /// <summary>
    /// Handle element animation started event
    /// </summary>
    private void OnElementAnimationStarted(object? sender, BootscreenElementEventArgs e)
    {
        // Element started animating
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
