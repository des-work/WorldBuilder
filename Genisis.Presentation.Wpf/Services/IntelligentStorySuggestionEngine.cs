using System.Collections.Concurrent;
using Genisis.Presentation.Wpf.ViewModels;
using Microsoft.Extensions.Logging;

namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Intelligent story suggestion engine with AI-powered recommendations
/// </summary>
public class IntelligentStorySuggestionEngine : IStorySuggestionEngine
{
    private readonly IAIEnhancementService _aiService;
    private readonly IStoryAnalyticsService _analyticsService;
    private readonly ILogger<IntelligentStorySuggestionEngine> _logger;
    private readonly ConcurrentDictionary<string, StorySuggestionCache> _suggestionCache = new();
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

    public IntelligentStorySuggestionEngine(
        IAIEnhancementService aiService,
        IStoryAnalyticsService analyticsService,
        ILogger<IntelligentStorySuggestionEngine> logger)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<StorySuggestion>> GenerateIntelligentSuggestionsAsync(
        StoryElement context, 
        UserPreferences preferences)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Generating intelligent suggestions for {Context}", context.Title);

            // Check cache first
            var cacheKey = GetCacheKey(context, preferences);
            if (_suggestionCache.TryGetValue(cacheKey, out var cachedSuggestions) && 
                !cachedSuggestions.IsExpired)
            {
                stopwatch.Stop();
                _logger.LogDebug("Returning cached suggestions for {Context} in {ElapsedMs}ms", 
                    context.Title, stopwatch.ElapsedMilliseconds);
                return cachedSuggestions.Suggestions;
            }

            await _processingSemaphore.WaitAsync();
            try
            {
                // Double-check cache after acquiring semaphore
                if (_suggestionCache.TryGetValue(cacheKey, out cachedSuggestions) && 
                    !cachedSuggestions.IsExpired)
                {
                    return cachedSuggestions.Suggestions;
                }

                // Analyze current story state
                var storyAnalysis = await _analyticsService.AnalyzeStoryAsync(context);
                
                // Generate AI suggestions
                var aiSuggestions = await _aiService.GenerateSuggestionsAsync(context);
                
                // Apply intelligence and user preferences
                var intelligentSuggestions = await ApplyIntelligenceAsync(
                    aiSuggestions, storyAnalysis, preferences, context);
                
                // Cache results
                _suggestionCache.TryAdd(cacheKey, new StorySuggestionCache
                {
                    Suggestions = intelligentSuggestions,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30)
                });

                stopwatch.Stop();
                _logger.LogDebug("Generated {Count} intelligent suggestions for {Context} in {ElapsedMs}ms", 
                    intelligentSuggestions.Count(), context.Title, stopwatch.ElapsedMilliseconds);

                return intelligentSuggestions;
            }
            finally
            {
                _processingSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to generate intelligent suggestions for {Context} after {ElapsedMs}ms", 
                context.Title, stopwatch.ElapsedMilliseconds);
            return Enumerable.Empty<StorySuggestion>();
        }
    }

    public async Task<IEnumerable<StorySuggestion>> GenerateContextualSuggestionsAsync(
        StoryElement primaryContext,
        StoryElement? secondaryContext,
        UserPreferences preferences)
    {
        try
        {
            _logger.LogDebug("Generating contextual suggestions for {Primary} and {Secondary}", 
                primaryContext.Title, secondaryContext?.Title ?? "None");

            var suggestions = new List<StorySuggestion>();

            // Generate suggestions based on primary context
            var primarySuggestions = await GenerateIntelligentSuggestionsAsync(primaryContext, preferences);
            suggestions.AddRange(primarySuggestions);

            // Generate cross-context suggestions if secondary context exists
            if (secondaryContext != null)
            {
                var crossContextSuggestions = await GenerateCrossContextSuggestionsAsync(
                    primaryContext, secondaryContext, preferences);
                suggestions.AddRange(crossContextSuggestions);
            }

            // Remove duplicates and sort by priority
            var uniqueSuggestions = suggestions
                .GroupBy(s => s.Title)
                .Select(g => g.OrderByDescending(s => s.Priority).First())
                .OrderByDescending(s => s.Priority)
                .ThenByDescending(s => s.Confidence)
                .Take(10);

            return uniqueSuggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate contextual suggestions");
            return Enumerable.Empty<StorySuggestion>();
        }
    }

    public async Task<StorySuggestion> EnhanceSuggestionAsync(
        StorySuggestion suggestion,
        StoryElement context,
        UserPreferences preferences)
    {
        try
        {
            _logger.LogDebug("Enhancing suggestion {Suggestion}", suggestion.Title);

            // Analyze suggestion context
            var analysis = await _analyticsService.AnalyzeSuggestionAsync(suggestion, context);
            
            // Enhance based on analysis
            var enhancedSuggestion = new StorySuggestion
            {
                Title = suggestion.Title,
                Description = suggestion.Description,
                Confidence = Math.Min(1.0, suggestion.Confidence + analysis.QualityScore * 0.1),
                Priority = suggestion.Priority + (int)(analysis.UrgencyScore * 2),
                Category = suggestion.Category,
                EstimatedTime = EstimateCompletionTime(suggestion, analysis),
                RequiredElements = GetRequiredElements(suggestion, analysis),
                Tags = suggestion.Tags.Concat(analysis.SuggestedTags).Distinct().ToList(),
                Difficulty = CalculateDifficulty(suggestion, analysis),
                Impact = CalculateImpact(suggestion, analysis)
            };

            return enhancedSuggestion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enhance suggestion {Suggestion}", suggestion.Title);
            return suggestion; // Return original if enhancement fails
        }
    }

    private async Task<IEnumerable<StorySuggestion>> ApplyIntelligenceAsync(
        IEnumerable<AISuggestion> aiSuggestions,
        StoryAnalysis storyAnalysis,
        UserPreferences preferences,
        StoryElement context)
    {
        var suggestions = new List<StorySuggestion>();

        foreach (var aiSuggestion in aiSuggestions)
        {
            var confidence = CalculateConfidence(aiSuggestion, storyAnalysis, preferences);
            var priority = CalculatePriority(aiSuggestion, storyAnalysis, preferences);
            var estimatedTime = EstimateCompletionTime(aiSuggestion);
            var requiredElements = GetRequiredElements(aiSuggestion);
            var tags = GenerateTags(aiSuggestion, context);
            var difficulty = CalculateDifficulty(aiSuggestion, storyAnalysis);
            var impact = CalculateImpact(aiSuggestion, storyAnalysis);

            suggestions.Add(new StorySuggestion
            {
                Title = aiSuggestion.Title,
                Description = aiSuggestion.Prompt,
                Confidence = confidence,
                Priority = priority,
                Category = aiSuggestion.Category,
                EstimatedTime = estimatedTime,
                RequiredElements = requiredElements,
                Tags = tags,
                Difficulty = difficulty,
                Impact = impact
            });
        }

        return suggestions.OrderByDescending(s => s.Priority)
                         .ThenByDescending(s => s.Confidence);
    }

    private async Task<IEnumerable<StorySuggestion>> GenerateCrossContextSuggestionsAsync(
        StoryElement primaryContext,
        StoryElement secondaryContext,
        UserPreferences preferences)
    {
        try
        {
            var suggestions = new List<StorySuggestion>();

            // Generate relationship-based suggestions
            if (primaryContext.ElementType == ElementType.Character && 
                secondaryContext.ElementType == ElementType.Story)
            {
                suggestions.Add(new StorySuggestion
                {
                    Title = "Character-Story Integration",
                    Description = $"Develop {primaryContext.Title}'s role in {secondaryContext.Title}",
                    Confidence = 0.9,
                    Priority = 8,
                    Category = "Character Development",
                    EstimatedTime = TimeSpan.FromMinutes(30),
                    RequiredElements = new List<string> { primaryContext.Title, secondaryContext.Title },
                    Tags = new List<string> { "character", "story", "integration" },
                    Difficulty = DifficultyLevel.Medium,
                    Impact = ImpactLevel.High
                });
            }

            // Generate conflict-based suggestions
            if (primaryContext.ElementType == ElementType.Character && 
                secondaryContext.ElementType == ElementType.Character)
            {
                suggestions.Add(new StorySuggestion
                {
                    Title = "Character Conflict",
                    Description = $"Create conflict between {primaryContext.Title} and {secondaryContext.Title}",
                    Confidence = 0.8,
                    Priority = 7,
                    Category = "Conflict Development",
                    EstimatedTime = TimeSpan.FromMinutes(45),
                    RequiredElements = new List<string> { primaryContext.Title, secondaryContext.Title },
                    Tags = new List<string> { "conflict", "characters", "tension" },
                    Difficulty = DifficultyLevel.Medium,
                    Impact = ImpactLevel.High
                });
            }

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate cross-context suggestions");
            return Enumerable.Empty<StorySuggestion>();
        }
    }

    private double CalculateConfidence(AISuggestion suggestion, StoryAnalysis analysis, UserPreferences preferences)
    {
        var baseConfidence = 0.5;
        
        // Adjust based on story coherence
        baseConfidence += analysis.CoherenceScore * 0.2;
        
        // Adjust based on user preferences
        if (preferences.PreferredCategories.Contains(suggestion.Category))
            baseConfidence += 0.2;
        
        // Adjust based on story gaps
        if (analysis.IdentifiedGaps.Any(gap => gap.Category == suggestion.Category))
            baseConfidence += 0.1;
        
        return Math.Min(1.0, baseConfidence);
    }

    private int CalculatePriority(AISuggestion suggestion, StoryAnalysis analysis, UserPreferences preferences)
    {
        var basePriority = 5;
        
        // Adjust based on story urgency
        basePriority += (int)(analysis.UrgencyScore * 2);
        
        // Adjust based on user preferences
        if (preferences.PreferredCategories.Contains(suggestion.Category))
            basePriority += 2;
        
        // Adjust based on story gaps
        if (analysis.IdentifiedGaps.Any(gap => gap.Category == suggestion.Category))
            basePriority += 3;
        
        return Math.Min(10, Math.Max(1, basePriority));
    }

    private TimeSpan EstimateCompletionTime(AISuggestion suggestion)
    {
        return suggestion.Category switch
        {
            "Character Development" => TimeSpan.FromMinutes(30),
            "Plot Development" => TimeSpan.FromMinutes(45),
            "World Building" => TimeSpan.FromMinutes(60),
            "Dialogue" => TimeSpan.FromMinutes(20),
            "Description" => TimeSpan.FromMinutes(15),
            _ => TimeSpan.FromMinutes(30)
        };
    }

    private TimeSpan EstimateCompletionTime(StorySuggestion suggestion, SuggestionAnalysis analysis)
    {
        var baseTime = suggestion.EstimatedTime;
        
        // Adjust based on difficulty
        var difficultyMultiplier = suggestion.Difficulty switch
        {
            DifficultyLevel.Easy => 0.8,
            DifficultyLevel.Medium => 1.0,
            DifficultyLevel.Hard => 1.5,
            _ => 1.0
        };
        
        // Adjust based on user experience
        var experienceMultiplier = analysis.UserExperienceLevel switch
        {
            ExperienceLevel.Beginner => 1.5,
            ExperienceLevel.Intermediate => 1.0,
            ExperienceLevel.Advanced => 0.7,
            _ => 1.0
        };
        
        return TimeSpan.FromMilliseconds(baseTime.TotalMilliseconds * difficultyMultiplier * experienceMultiplier);
    }

    private List<string> GetRequiredElements(AISuggestion suggestion)
    {
        return suggestion.Category switch
        {
            "Character Development" => new List<string> { "Character", "Backstory", "Motivation" },
            "Plot Development" => new List<string> { "Story", "Conflict", "Resolution" },
            "World Building" => new List<string> { "Universe", "Location", "Culture" },
            "Dialogue" => new List<string> { "Character", "Scene", "Context" },
            "Description" => new List<string> { "Setting", "Mood", "Details" },
            _ => new List<string>()
        };
    }

    private List<string> GetRequiredElements(StorySuggestion suggestion, SuggestionAnalysis analysis)
    {
        var elements = suggestion.RequiredElements.ToList();
        
        // Add elements based on analysis
        if (analysis.MissingElements.Any())
            elements.AddRange(analysis.MissingElements);
        
        return elements.Distinct().ToList();
    }

    private List<string> GenerateTags(AISuggestion suggestion, StoryElement context)
    {
        var tags = new List<string> { suggestion.Category.ToLower() };
        
        // Add context-specific tags
        tags.Add(context.ElementType.ToString().ToLower());
        
        // Add suggestion-specific tags
        if (suggestion.Title.Contains("Conflict"))
            tags.Add("conflict");
        if (suggestion.Title.Contains("Character"))
            tags.Add("character");
        if (suggestion.Title.Contains("Plot"))
            tags.Add("plot");
        
        return tags.Distinct().ToList();
    }

    private DifficultyLevel CalculateDifficulty(AISuggestion suggestion, StoryAnalysis analysis)
    {
        var difficulty = DifficultyLevel.Medium;
        
        // Adjust based on story complexity
        if (analysis.ComplexityScore > 0.7)
            difficulty = DifficultyLevel.Hard;
        else if (analysis.ComplexityScore < 0.3)
            difficulty = DifficultyLevel.Easy;
        
        // Adjust based on suggestion category
        if (suggestion.Category == "World Building" || suggestion.Category == "Plot Development")
            difficulty = DifficultyLevel.Hard;
        else if (suggestion.Category == "Description" || suggestion.Category == "Dialogue")
            difficulty = DifficultyLevel.Easy;
        
        return difficulty;
    }

    private DifficultyLevel CalculateDifficulty(StorySuggestion suggestion, SuggestionAnalysis analysis)
    {
        var difficulty = suggestion.Difficulty;
        
        // Adjust based on user experience
        if (analysis.UserExperienceLevel == ExperienceLevel.Beginner)
            difficulty = DifficultyLevel.Easy;
        else if (analysis.UserExperienceLevel == ExperienceLevel.Advanced)
            difficulty = DifficultyLevel.Hard;
        
        return difficulty;
    }

    private ImpactLevel CalculateImpact(AISuggestion suggestion, StoryAnalysis analysis)
    {
        var impact = ImpactLevel.Medium;
        
        // Adjust based on story importance
        if (analysis.ImportanceScore > 0.7)
            impact = ImpactLevel.High;
        else if (analysis.ImportanceScore < 0.3)
            impact = ImpactLevel.Low;
        
        // Adjust based on suggestion category
        if (suggestion.Category == "Plot Development" || suggestion.Category == "Character Development")
            impact = ImpactLevel.High;
        else if (suggestion.Category == "Description")
            impact = ImpactLevel.Low;
        
        return impact;
    }

    private ImpactLevel CalculateImpact(StorySuggestion suggestion, SuggestionAnalysis analysis)
    {
        var impact = suggestion.Impact;
        
        // Adjust based on analysis
        if (analysis.PotentialImpact > 0.7)
            impact = ImpactLevel.High;
        else if (analysis.PotentialImpact < 0.3)
            impact = ImpactLevel.Low;
        
        return impact;
    }

    private string GetCacheKey(StoryElement context, UserPreferences preferences)
    {
        return $"{context.Id}_{context.ElementType}_{preferences.GetHashCode()}";
    }

    /// <summary>
    /// Clear expired cache entries
    /// </summary>
    public void ClearExpiredCache()
    {
        var expiredKeys = _suggestionCache
            .Where(kvp => kvp.Value.IsExpired)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _suggestionCache.TryRemove(key, out _);
        }

        _logger.LogDebug("Cleared {Count} expired cache entries", expiredKeys.Count);
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    public CacheStatistics GetCacheStatistics()
    {
        var totalEntries = _suggestionCache.Count;
        var expiredEntries = _suggestionCache.Count(kvp => kvp.Value.IsExpired);
        var activeEntries = totalEntries - expiredEntries;

        return new CacheStatistics
        {
            TotalEntries = totalEntries,
            ActiveEntries = activeEntries,
            ExpiredEntries = expiredEntries,
            HitRate = CalculateHitRate()
        };
    }

    private double CalculateHitRate()
    {
        // This would be implemented with actual hit/miss tracking
        return 0.85; // Placeholder
    }
}

/// <summary>
/// Story suggestion cache entry
/// </summary>
public class StorySuggestionCache
{
    public IEnumerable<StorySuggestion> Suggestions { get; set; } = Enumerable.Empty<StorySuggestion>();
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}

/// <summary>
/// Cache statistics
/// </summary>
public class CacheStatistics
{
    public int TotalEntries { get; set; }
    public int ActiveEntries { get; set; }
    public int ExpiredEntries { get; set; }
    public double HitRate { get; set; }
}
