namespace Genisis.Presentation.Wpf.ViewModels;

/// <summary>
/// Enhanced story suggestion with intelligent analysis
/// </summary>
public class StorySuggestion
{
    /// <summary>
    /// Suggestion title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Suggestion description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Priority level (1 to 10)
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Suggestion category
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Estimated completion time
    /// </summary>
    public TimeSpan EstimatedTime { get; set; }

    /// <summary>
    /// Required elements for implementation
    /// </summary>
    public IEnumerable<string> RequiredElements { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Suggestion tags
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Difficulty level
    /// </summary>
    public DifficultyLevel Difficulty { get; set; }

    /// <summary>
    /// Impact level
    /// </summary>
    public ImpactLevel Impact { get; set; }

    /// <summary>
    /// Suggestion ID
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether suggestion has been applied
    /// </summary>
    public bool IsApplied { get; set; }

    /// <summary>
    /// User rating (1 to 5)
    /// </summary>
    public int UserRating { get; set; }

    /// <summary>
    /// User feedback
    /// </summary>
    public string UserFeedback { get; set; } = string.Empty;
}

/// <summary>
/// Difficulty levels
/// </summary>
public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}

/// <summary>
/// Impact levels
/// </summary>
public enum ImpactLevel
{
    Low,
    Medium,
    High
}

/// <summary>
/// User preferences for suggestions
/// </summary>
public class UserPreferences
{
    /// <summary>
    /// Preferred categories
    /// </summary>
    public List<string> PreferredCategories { get; set; } = new();

    /// <summary>
    /// Preferred difficulty levels
    /// </summary>
    public List<DifficultyLevel> PreferredDifficulties { get; set; } = new();

    /// <summary>
    /// Preferred impact levels
    /// </summary>
    public List<ImpactLevel> PreferredImpacts { get; set; } = new();

    /// <summary>
    /// Maximum estimated time
    /// </summary>
    public TimeSpan MaxEstimatedTime { get; set; } = TimeSpan.FromHours(2);

    /// <summary>
    /// Minimum confidence threshold
    /// </summary>
    public double MinConfidenceThreshold { get; set; } = 0.5;

    /// <summary>
    /// Minimum priority threshold
    /// </summary>
    public int MinPriorityThreshold { get; set; } = 5;

    /// <summary>
    /// Preferred tags
    /// </summary>
    public List<string> PreferredTags { get; set; } = new();

    /// <summary>
    /// Excluded tags
    /// </summary>
    public List<string> ExcludedTags { get; set; } = new();
}

/// <summary>
/// Story analysis result
/// </summary>
public class StoryAnalysis
{
    /// <summary>
    /// Coherence score (0.0 to 1.0)
    /// </summary>
    public double CoherenceScore { get; set; }

    /// <summary>
    /// Complexity score (0.0 to 1.0)
    /// </summary>
    public double ComplexityScore { get; set; }

    /// <summary>
    /// Urgency score (0.0 to 1.0)
    /// </summary>
    public double UrgencyScore { get; set; }

    /// <summary>
    /// Importance score (0.0 to 1.0)
    /// </summary>
    public double ImportanceScore { get; set; }

    /// <summary>
    /// Identified gaps
    /// </summary>
    public List<StoryGap> IdentifiedGaps { get; set; } = new();

    /// <summary>
    /// Strengths
    /// </summary>
    public List<string> Strengths { get; set; } = new();

    /// <summary>
    /// Weaknesses
    /// </summary>
    public List<string> Weaknesses { get; set; } = new();

    /// <summary>
    /// Recommendations
    /// </summary>
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Story gap
/// </summary>
public class StoryGap
{
    /// <summary>
    /// Gap category
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gap description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gap severity
    /// </summary>
    public GapSeverity Severity { get; set; }

    /// <summary>
    /// Suggested fixes
    /// </summary>
    public List<string> SuggestedFixes { get; set; } = new();
}

/// <summary>
/// Gap severity levels
/// </summary>
public enum GapSeverity
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Suggestion analysis result
/// </summary>
public class SuggestionAnalysis
{
    /// <summary>
    /// Quality score (0.0 to 1.0)
    /// </summary>
    public double QualityScore { get; set; }

    /// <summary>
    /// Urgency score (0.0 to 1.0)
    /// </summary>
    public double UrgencyScore { get; set; }

    /// <summary>
    /// User experience level
    /// </summary>
    public ExperienceLevel UserExperienceLevel { get; set; }

    /// <summary>
    /// Missing elements
    /// </summary>
    public List<string> MissingElements { get; set; } = new();

    /// <summary>
    /// Suggested tags
    /// </summary>
    public List<string> SuggestedTags { get; set; } = new();

    /// <summary>
    /// Potential impact (0.0 to 1.0)
    /// </summary>
    public double PotentialImpact { get; set; }

    /// <summary>
    /// Implementation complexity (0.0 to 1.0)
    /// </summary>
    public double ImplementationComplexity { get; set; }
}

/// <summary>
/// Experience levels
/// </summary>
public enum ExperienceLevel
{
    Beginner,
    Intermediate,
    Advanced
}

/// <summary>
/// Consistency analysis result
/// </summary>
public class ConsistencyAnalysis
{
    /// <summary>
    /// Overall consistency score (0.0 to 1.0)
    /// </summary>
    public double ConsistencyScore { get; set; }

    /// <summary>
    /// Inconsistencies found
    /// </summary>
    public List<Inconsistency> Inconsistencies { get; set; } = new();

    /// <summary>
    /// Recommendations for improvement
    /// </summary>
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Inconsistency details
/// </summary>
public class Inconsistency
{
    /// <summary>
    /// Inconsistency type
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Affected elements
    /// </summary>
    public List<string> AffectedElements { get; set; } = new();

    /// <summary>
    /// Severity
    /// </summary>
    public InconsistencySeverity Severity { get; set; }
}

/// <summary>
/// Inconsistency severity levels
/// </summary>
public enum InconsistencySeverity
{
    Minor,
    Moderate,
    Major,
    Critical
}

/// <summary>
/// Pacing analysis result
/// </summary>
public class PacingAnalysis
{
    /// <summary>
    /// Overall pacing score (0.0 to 1.0)
    /// </summary>
    public double PacingScore { get; set; }

    /// <summary>
    /// Pacing issues
    /// </summary>
    public List<PacingIssue> PacingIssues { get; set; } = new();

    /// <summary>
    /// Recommendations
    /// </summary>
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Pacing issue details
/// </summary>
public class PacingIssue
{
    /// <summary>
    /// Issue type
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Location in story
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Severity
    /// </summary>
    public PacingIssueSeverity Severity { get; set; }
}

/// <summary>
/// Pacing issue severity levels
/// </summary>
public enum PacingIssueSeverity
{
    Low,
    Medium,
    High
}

/// <summary>
/// Character development analysis result
/// </summary>
public class CharacterDevelopmentAnalysis
{
    /// <summary>
    /// Development score (0.0 to 1.0)
    /// </summary>
    public double DevelopmentScore { get; set; }

    /// <summary>
    /// Character arc analysis
    /// </summary>
    public CharacterArcAnalysis ArcAnalysis { get; set; } = new();

    /// <summary>
    /// Development opportunities
    /// </summary>
    public List<string> DevelopmentOpportunities { get; set; } = new();

    /// <summary>
    /// Strengths
    /// </summary>
    public List<string> Strengths { get; set; } = new();

    /// <summary>
    /// Weaknesses
    /// </summary>
    public List<string> Weaknesses { get; set; } = new();
}

/// <summary>
/// Character arc analysis
/// </summary>
public class CharacterArcAnalysis
{
    /// <summary>
    /// Arc type
    /// </summary>
    public string ArcType { get; set; } = string.Empty;

    /// <summary>
    /// Arc completeness (0.0 to 1.0)
    /// </summary>
    public double Completeness { get; set; }

    /// <summary>
    /// Key moments
    /// </summary>
    public List<string> KeyMoments { get; set; } = new();

    /// <summary>
    /// Missing elements
    /// </summary>
    public List<string> MissingElements { get; set; } = new();
}
