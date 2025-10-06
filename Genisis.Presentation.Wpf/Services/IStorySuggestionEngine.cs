using Genisis.Presentation.Wpf.ViewModels;

namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Interface for intelligent story suggestion engine
/// </summary>
public interface IStorySuggestionEngine
{
    /// <summary>
    /// Generate intelligent suggestions for story element
    /// </summary>
    /// <param name="context">Story element context</param>
    /// <param name="preferences">User preferences</param>
    /// <returns>Intelligent story suggestions</returns>
    Task<IEnumerable<StorySuggestion>> GenerateIntelligentSuggestionsAsync(
        StoryElement context, 
        UserPreferences preferences);

    /// <summary>
    /// Generate contextual suggestions for multiple elements
    /// </summary>
    /// <param name="primaryContext">Primary context element</param>
    /// <param name="secondaryContext">Secondary context element</param>
    /// <param name="preferences">User preferences</param>
    /// <returns>Contextual story suggestions</returns>
    Task<IEnumerable<StorySuggestion>> GenerateContextualSuggestionsAsync(
        StoryElement primaryContext,
        StoryElement? secondaryContext,
        UserPreferences preferences);

    /// <summary>
    /// Enhance an existing suggestion
    /// </summary>
    /// <param name="suggestion">Suggestion to enhance</param>
    /// <param name="context">Story context</param>
    /// <param name="preferences">User preferences</param>
    /// <returns>Enhanced suggestion</returns>
    Task<StorySuggestion> EnhanceSuggestionAsync(
        StorySuggestion suggestion,
        StoryElement context,
        UserPreferences preferences);
}
