using System.Windows;
using System.Windows.Media;
using Genisis.Presentation.Wpf.Themes;

namespace Genisis.Presentation.Wpf.Bootscreen;

/// <summary>
/// Interface for bootscreen elements that can be composed together
/// </summary>
public interface IBootscreenElement
{
    /// <summary>
    /// Element identifier
    /// </summary>
    string ElementId { get; }

    /// <summary>
    /// Element name for debugging
    /// </summary>
    string ElementName { get; }

    /// <summary>
    /// Whether the element is currently visible
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Element's visual representation
    /// </summary>
    FrameworkElement VisualElement { get; }

    /// <summary>
    /// Initialize the element
    /// </summary>
    void Initialize();

    /// <summary>
    /// Start the element's animation sequence
    /// </summary>
    Task StartAnimationAsync(IThemeProvider theme, TimeSpan delay = default);

    /// <summary>
    /// Update element with new theme
    /// </summary>
    void UpdateTheme(IThemeProvider theme);

    /// <summary>
    /// Stop the element's animation
    /// </summary>
    void StopAnimation();

    /// <summary>
    /// Cleanup element resources
    /// </summary>
    void Cleanup();

    /// <summary>
    /// Event raised when element animation completes
    /// </summary>
    event EventHandler<BootscreenElementEventArgs>? AnimationCompleted;

    /// <summary>
    /// Event raised when element animation starts
    /// </summary>
    event EventHandler<BootscreenElementEventArgs>? AnimationStarted;
}

/// <summary>
/// Event args for bootscreen element events
/// </summary>
public class BootscreenElementEventArgs : EventArgs
{
    public IBootscreenElement Element { get; }
    public string EventType { get; }

    public BootscreenElementEventArgs(IBootscreenElement element, string eventType)
    {
        Element = element;
        EventType = eventType;
    }
}
