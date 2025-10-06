using Genisis.Presentation.Wpf.ViewModels;

namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Enhanced AI service for intelligent story assistance
/// </summary>
public interface IAIEnhancementService
{
    /// <summary>
    /// Generate AI suggestions for story element
    /// </summary>
    /// <param name="context">Story element context</param>
    /// <returns>AI suggestions</returns>
    Task<IEnumerable<AISuggestion>> GenerateSuggestionsAsync(StoryElement context);

    /// <summary>
    /// Generate dialogue between characters
    /// </summary>
    /// <param name="character1">First character</param>
    /// <param name="character2">Second character</param>
    /// <returns>Generated dialogue</returns>
    Task<string> GenerateDialogueAsync(StoryElement character1, StoryElement character2);

    /// <summary>
    /// Analyze relationships between elements
    /// </summary>
    /// <param name="element">Element to analyze</param>
    /// <returns>Relationship analysis</returns>
    Task<string> AnalyzeRelationshipsAsync(StoryElement element);

    /// <summary>
    /// Suggest plot development
    /// </summary>
    /// <param name="element">Story element</param>
    /// <returns>Plot development suggestions</returns>
    Task<string> SuggestPlotDevelopmentAsync(StoryElement element);

    /// <summary>
    /// Test character voice
    /// </summary>
    /// <param name="character">Character to test</param>
    /// <param name="prompt">Test prompt</param>
    /// <returns>Character voice response</returns>
    Task<string> TestCharacterVoiceAsync(StoryElement character, string prompt);

    /// <summary>
    /// Generate world-building suggestions
    /// </summary>
    /// <param name="universe">Universe element</param>
    /// <returns>World-building suggestions</returns>
    Task<string> GenerateWorldBuildingSuggestionsAsync(StoryElement universe);

    /// <summary>
    /// Analyze story consistency
    /// </summary>
    /// <param name="elements">Story elements to analyze</param>
    /// <returns>Consistency analysis</returns>
    Task<string> AnalyzeStoryConsistencyAsync(IEnumerable<StoryElement> elements);

    /// <summary>
    /// Generate character development suggestions
    /// </summary>
    /// <param name="character">Character element</param>
    /// <returns>Character development suggestions</returns>
    Task<string> GenerateCharacterDevelopmentSuggestionsAsync(StoryElement character);

    /// <summary>
    /// Generate conflict ideas
    /// </summary>
    /// <param name="context">Story context</param>
    /// <returns>Conflict suggestions</returns>
    Task<string> GenerateConflictIdeasAsync(StoryElement context);

    /// <summary>
    /// Generate resolution ideas
    /// </summary>
    /// <param name="conflict">Conflict element</param>
    /// <returns>Resolution suggestions</returns>
    Task<string> GenerateResolutionIdeasAsync(StoryElement conflict);

    /// <summary>
    /// Analyze pacing
    /// </summary>
    /// <param name="story">Story element</param>
    /// <returns>Pacing analysis</returns>
    Task<string> AnalyzePacingAsync(StoryElement story);

    /// <summary>
    /// Generate theme exploration ideas
    /// </summary>
    /// <param name="story">Story element</param>
    /// <returns>Theme exploration suggestions</returns>
    Task<string> GenerateThemeExplorationIdeasAsync(StoryElement story);
}
