using Genisis.Presentation.Wpf.ViewModels;

namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Interface for story analytics service
/// </summary>
public interface IStoryAnalyticsService
{
    /// <summary>
    /// Analyze story element
    /// </summary>
    /// <param name="element">Story element to analyze</param>
    /// <returns>Story analysis</returns>
    Task<StoryAnalysis> AnalyzeStoryAsync(StoryElement element);

    /// <summary>
    /// Analyze suggestion
    /// </summary>
    /// <param name="suggestion">Suggestion to analyze</param>
    /// <param name="context">Story context</param>
    /// <returns>Suggestion analysis</returns>
    Task<SuggestionAnalysis> AnalyzeSuggestionAsync(StorySuggestion suggestion, StoryElement context);

    /// <summary>
    /// Analyze story consistency
    /// </summary>
    /// <param name="elements">Story elements to analyze</param>
    /// <returns>Consistency analysis</returns>
    Task<ConsistencyAnalysis> AnalyzeConsistencyAsync(IEnumerable<StoryElement> elements);

    /// <summary>
    /// Analyze story pacing
    /// </summary>
    /// <param name="story">Story to analyze</param>
    /// <returns>Pacing analysis</returns>
    Task<PacingAnalysis> AnalyzePacingAsync(StoryElement story);

    /// <summary>
    /// Analyze character development
    /// </summary>
    /// <param name="character">Character to analyze</param>
    /// <returns>Character development analysis</returns>
    Task<CharacterDevelopmentAnalysis> AnalyzeCharacterDevelopmentAsync(StoryElement character);
}
