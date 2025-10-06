using System;
using System.Collections.Generic;

namespace Genisis.Core.Models;

/// <summary>
/// Smart query representation
/// </summary>
public class SmartQuery
{
    /// <summary>
    /// User input
    /// </summary>
    public string UserInput { get; set; } = string.Empty;

    /// <summary>
    /// Detected intent
    /// </summary>
    public QueryIntent Intent { get; set; }

    /// <summary>
    /// Query context
    /// </summary>
    public QueryContext Context { get; set; } = new();

    /// <summary>
    /// User preferences
    /// </summary>
    public QueryPreferences Preferences { get; set; } = new();

    /// <summary>
    /// Query constraints
    /// </summary>
    public QueryConstraints Constraints { get; set; } = new();

    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Session ID
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Query ID
    /// </summary>
    public string QueryId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Query metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Query priority
    /// </summary>
    public QueryPriority Priority { get; set; } = QueryPriority.Normal;

    /// <summary>
    /// Query source
    /// </summary>
    public QuerySource Source { get; set; } = QuerySource.User;

    /// <summary>
    /// Query language
    /// </summary>
    public string Language { get; set; } = "en-US";
}

/// <summary>
/// Query intents
/// </summary>
public enum QueryIntent
{
    CharacterDevelopment,
    PlotDevelopment,
    WorldBuilding,
    DialogueGeneration,
    DescriptionWriting,
    ConflictCreation,
    ResolutionPlanning,
    Analysis,
    Suggestion,
    Question,
    Command,
    CreativeWriting,
    CodeGeneration,
    Translation,
    Summarization,
    Explanation,
    Comparison,
    Evaluation,
    Planning,
    Brainstorming
}

/// <summary>
/// Query context
/// </summary>
public class QueryContext
{
    /// <summary>
    /// Primary story element
    /// </summary>
    public StoryElement? PrimaryElement { get; set; }

    /// <summary>
    /// Secondary story element
    /// </summary>
    public StoryElement? SecondaryElement { get; set; }

    /// <summary>
    /// Tertiary story element
    /// </summary>
    public StoryElement? TertiaryElement { get; set; }

    /// <summary>
    /// Related story elements
    /// </summary>
    public List<StoryElement> RelatedElements { get; set; } = new();

    /// <summary>
    /// User preferences
    /// </summary>
    public UserPreferences UserPreferences { get; set; } = new();

    /// <summary>
    /// Conversation history
    /// </summary>
    public ConversationHistory ConversationHistory { get; set; } = new();

    /// <summary>
    /// Current story state
    /// </summary>
    public StoryState StoryState { get; set; } = new();

    /// <summary>
    /// Context scope
    /// </summary>
    public ContextScope Scope { get; set; } = ContextScope.ElementOnly;

    /// <summary>
    /// Context depth
    /// </summary>
    public int Depth { get; set; } = 1;

    /// <summary>
    /// Context relevance score
    /// </summary>
    public double RelevanceScore { get; set; } = 1.0;

    /// <summary>
    /// Context timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Query preferences
/// </summary>
public class QueryPreferences
{
    /// <summary>
    /// Preferred response style
    /// </summary>
    public ResponseStyle ResponseStyle { get; set; } = ResponseStyle.Balanced;

    /// <summary>
    /// Preferred detail level
    /// </summary>
    public DetailLevel DetailLevel { get; set; } = DetailLevel.Medium;

    /// <summary>
    /// Preferred creativity level
    /// </summary>
    public CreativityLevel CreativityLevel { get; set; } = CreativityLevel.Medium;

    /// <summary>
    /// Preferred formality level
    /// </summary>
    public FormalityLevel FormalityLevel { get; set; } = FormalityLevel.Casual;

    /// <summary>
    /// Maximum response length
    /// </summary>
    public int MaxResponseLength { get; set; } = 1000;

    /// <summary>
    /// Include examples
    /// </summary>
    public bool IncludeExamples { get; set; } = true;

    /// <summary>
    /// Include suggestions
    /// </summary>
    public bool IncludeSuggestions { get; set; } = true;

    /// <summary>
    /// Include analysis
    /// </summary>
    public bool IncludeAnalysis { get; set; } = false;

    /// <summary>
    /// Custom preferences
    /// </summary>
    public Dictionary<string, object> CustomPreferences { get; set; } = new();
}

/// <summary>
/// Query constraints
/// </summary>
public class QueryConstraints
{
    /// <summary>
    /// Maximum processing time
    /// </summary>
    public TimeSpan MaxProcessingTime { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Maximum tokens
    /// </summary>
    public int MaxTokens { get; set; } = 2048;

    /// <summary>
    /// Maximum memory usage
    /// </summary>
    public long MaxMemoryUsageMB { get; set; } = 1024;

    /// <summary>
    /// Allowed models
    /// </summary>
    public List<string> AllowedModels { get; set; } = new();

    /// <summary>
    /// Disallowed models
    /// </summary>
    public List<string> DisallowedModels { get; set; } = new();

    /// <summary>
    /// Required capabilities
    /// </summary>
    public List<string> RequiredCapabilities { get; set; } = new();

    /// <summary>
    /// Quality requirements
    /// </summary>
    public QualityRequirements QualityRequirements { get; set; } = new();

    /// <summary>
    /// Performance requirements
    /// </summary>
    public PerformanceRequirements PerformanceRequirements { get; set; } = new();
}

/// <summary>
/// Query plan
/// </summary>
public class QueryPlan
{
    /// <summary>
    /// Plan ID
    /// </summary>
    public string PlanId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Plan steps
    /// </summary>
    public List<QueryStep> Steps { get; set; } = new();

    /// <summary>
    /// Selected model
    /// </summary>
    public string SelectedModel { get; set; } = string.Empty;

    /// <summary>
    /// Estimated duration
    /// </summary>
    public TimeSpan EstimatedDuration { get; set; }

    /// <summary>
    /// Confidence score
    /// </summary>
    public double ConfidenceScore { get; set; }

    /// <summary>
    /// Required resources
    /// </summary>
    public List<string> RequiredResources { get; set; } = new();

    /// <summary>
    /// Potential issues
    /// </summary>
    public List<string> PotentialIssues { get; set; } = new();

    /// <summary>
    /// Plan metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Plan priority
    /// </summary>
    public QueryPriority Priority { get; set; } = QueryPriority.Normal;

    /// <summary>
    /// Plan status
    /// </summary>
    public QueryPlanStatus Status { get; set; } = QueryPlanStatus.Created;
}

/// <summary>
/// Query step
/// </summary>
public class QueryStep
{
    /// <summary>
    /// Step name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Step type
    /// </summary>
    public QueryStepType Type { get; set; }

    /// <summary>
    /// Step parameters
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Estimated duration
    /// </summary>
    public TimeSpan EstimatedDuration { get; set; }

    /// <summary>
    /// Step dependencies
    /// </summary>
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// Whether step is optional
    /// </summary>
    public bool IsOptional { get; set; }

    /// <summary>
    /// Step status
    /// </summary>
    public QueryStepStatus Status { get; set; } = QueryStepStatus.Pending;

    /// <summary>
    /// Step result
    /// </summary>
    public object? Result { get; set; }

    /// <summary>
    /// Step error
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Step start time
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Step end time
    /// </summary>
    public DateTime? EndTime { get; set; }
}

/// <summary>
/// Query step types
/// </summary>
public enum QueryStepType
{
    ContextAnalysis,
    ModelSelection,
    PromptGeneration,
    QueryExecution,
    ResponseProcessing,
    ContextUpdate,
    SuggestionGeneration,
    Validation,
    Optimization,
    Caching,
    Logging
}

/// <summary>
/// Query response
/// </summary>
public class QueryResponse
{
    /// <summary>
    /// Response ID
    /// </summary>
    public string ResponseId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Response content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Response type
    /// </summary>
    public ResponseType Type { get; set; }

    /// <summary>
    /// Confidence score
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// AI suggestions
    /// </summary>
    public List<AISuggestion> Suggestions { get; set; } = new();

    /// <summary>
    /// Context references
    /// </summary>
    public List<ContextReference> ContextReferences { get; set; } = new();

    /// <summary>
    /// Performance metrics
    /// </summary>
    public PerformanceMetrics Performance { get; set; } = new();

    /// <summary>
    /// Response timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Model used
    /// </summary>
    public string ModelUsed { get; set; } = string.Empty;

    /// <summary>
    /// Response metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Response quality
    /// </summary>
    public ResponseQuality Quality { get; set; } = new();
}

/// <summary>
/// Response types
/// </summary>
public enum ResponseType
{
    Answer,
    Suggestion,
    Analysis,
    Generation,
    Question,
    Clarification,
    Error,
    Warning,
    Information,
    Confirmation
}

/// <summary>
/// Context reference
/// </summary>
public class ContextReference
{
    /// <summary>
    /// Referenced element
    /// </summary>
    public StoryElement Element { get; set; } = new();

    /// <summary>
    /// Reference text
    /// </summary>
    public string ReferenceText { get; set; } = string.Empty;

    /// <summary>
    /// Relevance score
    /// </summary>
    public double RelevanceScore { get; set; }

    /// <summary>
    /// Reference type
    /// </summary>
    public ReferenceType Type { get; set; }

    /// <summary>
    /// Reference position
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Reference length
    /// </summary>
    public int Length { get; set; }
}

/// <summary>
/// Reference types
/// </summary>
public enum ReferenceType
{
    Direct,
    Indirect,
    Implied,
    Background,
    Historical,
    Future,
    Related,
    Contrasting
}

/// <summary>
/// Query optimization
/// </summary>
public class QueryOptimization
{
    /// <summary>
    /// Original query
    /// </summary>
    public SmartQuery OriginalQuery { get; set; } = new();

    /// <summary>
    /// Optimized query
    /// </summary>
    public SmartQuery OptimizedQuery { get; set; } = new();

    /// <summary>
    /// Optimization changes
    /// </summary>
    public List<OptimizationChange> Changes { get; set; } = new();

    /// <summary>
    /// Optimization score
    /// </summary>
    public double OptimizationScore { get; set; }

    /// <summary>
    /// Expected improvement
    /// </summary>
    public OptimizationImprovement ExpectedImprovement { get; set; } = new();

    /// <summary>
    /// Optimization metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Optimization change
/// </summary>
public class OptimizationChange
{
    /// <summary>
    /// Change type
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Change description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Original value
    /// </summary>
    public object? OriginalValue { get; set; }

    /// <summary>
    /// Optimized value
    /// </summary>
    public object? OptimizedValue { get; set; }

    /// <summary>
    /// Impact score
    /// </summary>
    public double ImpactScore { get; set; }

    /// <summary>
    /// Change reason
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Optimization improvement
/// </summary>
public class OptimizationImprovement
{
    /// <summary>
    /// Performance improvement
    /// </summary>
    public double PerformanceImprovement { get; set; }

    /// <summary>
    /// Quality improvement
    /// </summary>
    public double QualityImprovement { get; set; }

    /// <summary>
    /// Cost reduction
    /// </summary>
    public double CostReduction { get; set; }

    /// <summary>
    /// Time reduction
    /// </summary>
    public TimeSpan TimeReduction { get; set; }

    /// <summary>
    /// Memory reduction
    /// </summary>
    public long MemoryReductionMB { get; set; }
}

/// <summary>
/// Query result
/// </summary>
public class QueryResult
{
    /// <summary>
    /// Result ID
    /// </summary>
    public string ResultId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Query response
    /// </summary>
    public QueryResponse Response { get; set; } = new();

    /// <summary>
    /// Execution time
    /// </summary>
    public TimeSpan ExecutionTime { get; set; }

    /// <summary>
    /// Success status
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Result metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Result timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Query intent analysis
/// </summary>
public class QueryIntentAnalysis
{
    /// <summary>
    /// Detected intent
    /// </summary>
    public QueryIntent Intent { get; set; }

    /// <summary>
    /// Intent confidence
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Intent keywords
    /// </summary>
    public List<string> Keywords { get; set; } = new();

    /// <summary>
    /// Intent patterns
    /// </summary>
    public List<string> Patterns { get; set; } = new();

    /// <summary>
    /// Intent context
    /// </summary>
    public Dictionary<string, object> Context { get; set; } = new();

    /// <summary>
    /// Alternative intents
    /// </summary>
    public List<QueryIntent> AlternativeIntents { get; set; } = new();

    /// <summary>
    /// Analysis metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Query suggestion
/// </summary>
public class QuerySuggestion
{
    /// <summary>
    /// Suggestion text
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Suggestion intent
    /// </summary>
    public QueryIntent Intent { get; set; }

    /// <summary>
    /// Suggestion confidence
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Suggestion category
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Suggestion priority
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Suggestion metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Query validation result
/// </summary>
public class QueryValidationResult
{
    /// <summary>
    /// Whether query is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Validation warnings
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Validation score
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Validation details
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();
}

/// <summary>
/// Query priorities
/// </summary>
public enum QueryPriority
{
    Low,
    Normal,
    High,
    Critical
}

/// <summary>
/// Query sources
/// </summary>
public enum QuerySource
{
    User,
    System,
    Automated,
    Scheduled,
    Background
}

/// <summary>
/// Query plan status
/// </summary>
public enum QueryPlanStatus
{
    Created,
    Validated,
    Optimized,
    Executing,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Query step status
/// </summary>
public enum QueryStepStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Skipped,
    Cancelled
}

/// <summary>
/// Response styles
/// </summary>
public enum ResponseStyle
{
    Creative,
    Analytical,
    Balanced,
    Technical,
    Casual,
    Formal
}

/// <summary>
/// Detail levels
/// </summary>
public enum DetailLevel
{
    Minimal,
    Low,
    Medium,
    High,
    Comprehensive
}

/// <summary>
/// Creativity levels
/// </summary>
public enum CreativityLevel
{
    Conservative,
    Moderate,
    Medium,
    High,
    Experimental
}

/// <summary>
/// Formality levels
/// </summary>
public enum FormalityLevel
{
    VeryCasual,
    Casual,
    Neutral,
    Formal,
    VeryFormal
}

/// <summary>
/// Quality requirements
/// </summary>
public class QualityRequirements
{
    /// <summary>
    /// Minimum coherence score
    /// </summary>
    public double MinCoherenceScore { get; set; } = 0.7;

    /// <summary>
    /// Minimum relevance score
    /// </summary>
    public double MinRelevanceScore { get; set; } = 0.7;

    /// <summary>
    /// Minimum creativity score
    /// </summary>
    public double MinCreativityScore { get; set; } = 0.5;

    /// <summary>
    /// Minimum accuracy score
    /// </summary>
    public double MinAccuracyScore { get; set; } = 0.8;
}

/// <summary>
/// Performance requirements
/// </summary>
public class PerformanceRequirements
{
    /// <summary>
    /// Maximum response time
    /// </summary>
    public TimeSpan MaxResponseTime { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Minimum tokens per second
    /// </summary>
    public double MinTokensPerSecond { get; set; } = 10.0;

    /// <summary>
    /// Maximum memory usage
    /// </summary>
    public long MaxMemoryUsageMB { get; set; } = 512;

    /// <summary>
    /// Maximum CPU usage
    /// </summary>
    public double MaxCpuUsagePercent { get; set; } = 80.0;
}

/// <summary>
/// Response quality
/// </summary>
public class ResponseQuality
{
    /// <summary>
    /// Coherence score
    /// </summary>
    public double CoherenceScore { get; set; }

    /// <summary>
    /// Relevance score
    /// </summary>
    public double RelevanceScore { get; set; }

    /// <summary>
    /// Creativity score
    /// </summary>
    public double CreativityScore { get; set; }

    /// <summary>
    /// Accuracy score
    /// </summary>
    public double AccuracyScore { get; set; }

    /// <summary>
    /// Completeness score
    /// </summary>
    public double CompletenessScore { get; set; }

    /// <summary>
    /// Overall quality score
    /// </summary>
    public double OverallScore { get; set; }
}
