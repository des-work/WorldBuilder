using Genisis.Presentation.Wpf.ViewModels;

namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Manages story element contexts for non-linear creation
/// </summary>
public interface IContextManager
{
    /// <summary>
    /// Primary context element
    /// </summary>
    StoryElement? PrimaryContext { get; }

    /// <summary>
    /// Secondary context element
    /// </summary>
    StoryElement? SecondaryContext { get; }

    /// <summary>
    /// Tertiary context element
    /// </summary>
    StoryElement? TertiaryContext { get; }

    /// <summary>
    /// Event fired when context changes
    /// </summary>
    event EventHandler<ContextChangedEventArgs>? ContextChanged;

    /// <summary>
    /// Set primary context
    /// </summary>
    /// <param name="element">Element to set as primary context</param>
    /// <returns>Task representing the operation</returns>
    Task SetPrimaryContextAsync(StoryElement element);

    /// <summary>
    /// Set secondary context
    /// </summary>
    /// <param name="element">Element to set as secondary context</param>
    /// <returns>Task representing the operation</returns>
    Task SetSecondaryContextAsync(StoryElement element);

    /// <summary>
    /// Set tertiary context
    /// </summary>
    /// <param name="element">Element to set as tertiary context</param>
    /// <returns>Task representing the operation</returns>
    Task SetTertiaryContextAsync(StoryElement element);

    /// <summary>
    /// Clear context
    /// </summary>
    /// <param name="element">Element to clear from context</param>
    /// <returns>Task representing the operation</returns>
    Task ClearContextAsync(StoryElement element);

    /// <summary>
    /// Clear all contexts
    /// </summary>
    /// <returns>Task representing the operation</returns>
    Task ClearAllContextsAsync();

    /// <summary>
    /// Get current context summary
    /// </summary>
    /// <returns>Context summary string</returns>
    string GetContextSummary();

    /// <summary>
    /// Check if element is in any context
    /// </summary>
    /// <param name="element">Element to check</param>
    /// <returns>True if element is in context</returns>
    bool IsInContext(StoryElement element);

    /// <summary>
    /// Get context level for element
    /// </summary>
    /// <param name="element">Element to check</param>
    /// <returns>Context level (Primary, Secondary, Tertiary, or None)</returns>
    ContextLevel GetContextLevel(StoryElement element);
}

/// <summary>
/// Event args for context changed events
/// </summary>
public class ContextChangedEventArgs : EventArgs
{
    /// <summary>
    /// Previous context
    /// </summary>
    public StoryElement? PreviousContext { get; }

    /// <summary>
    /// New context
    /// </summary>
    public StoryElement? NewContext { get; }

    /// <summary>
    /// Context level that changed
    /// </summary>
    public ContextLevel Level { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="previousContext">Previous context</param>
    /// <param name="newContext">New context</param>
    /// <param name="level">Context level</param>
    public ContextChangedEventArgs(StoryElement? previousContext, StoryElement? newContext, ContextLevel level)
    {
        PreviousContext = previousContext;
        NewContext = newContext;
        Level = level;
    }
}

/// <summary>
/// Context levels
/// </summary>
public enum ContextLevel
{
    None,
    Primary,
    Secondary,
    Tertiary
}
