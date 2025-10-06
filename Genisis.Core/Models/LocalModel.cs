using System;
using System.Collections.Generic;

namespace Genisis.Core.Models;

/// <summary>
/// Represents a local AI model
/// </summary>
public class LocalModel
{
    /// <summary>
    /// Model name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display name for UI
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Model size in bytes
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Model type
    /// </summary>
    public ModelType Type { get; set; }

    /// <summary>
    /// Model capabilities
    /// </summary>
    public ModelCapabilities Capabilities { get; set; } = new();

    /// <summary>
    /// Performance metrics
    /// </summary>
    public PerformanceMetrics Performance { get; set; } = new();

    /// <summary>
    /// Whether model is installed
    /// </summary>
    public bool IsInstalled { get; set; }

    /// <summary>
    /// Whether model is recommended
    /// </summary>
    public bool IsRecommended { get; set; }

    /// <summary>
    /// Last time model was used
    /// </summary>
    public DateTime LastUsed { get; set; }

    /// <summary>
    /// Number of times model has been used
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// Model description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Model version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Model tags
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Model configuration
    /// </summary>
    public ModelConfiguration Configuration { get; set; } = new();

    /// <summary>
    /// Whether model is currently available
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Model health status
    /// </summary>
    public ModelHealthStatus HealthStatus { get; set; }

    /// <summary>
    /// Last health check timestamp
    /// </summary>
    public DateTime LastHealthCheck { get; set; }

    /// <summary>
    /// Model installation path
    /// </summary>
    public string InstallationPath { get; set; } = string.Empty;

    /// <summary>
    /// Model dependencies
    /// </summary>
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// Model requirements
    /// </summary>
    public ModelRequirements Requirements { get; set; } = new();
}

/// <summary>
/// Model types
/// </summary>
public enum ModelType
{
    TextGeneration,
    CodeGeneration,
    CreativeWriting,
    Analysis,
    Conversation,
    Multimodal,
    Specialized,
    General
}

/// <summary>
/// Model capabilities
/// </summary>
public class ModelCapabilities
{
    /// <summary>
    /// Maximum context length
    /// </summary>
    public int MaxContextLength { get; set; }

    /// <summary>
    /// Whether model supports streaming
    /// </summary>
    public bool SupportsStreaming { get; set; }

    /// <summary>
    /// Whether model supports function calling
    /// </summary>
    public bool SupportsFunctionCalling { get; set; }

    /// <summary>
    /// Whether model supports multimodal input
    /// </summary>
    public bool SupportsMultimodal { get; set; }

    /// <summary>
    /// Supported languages
    /// </summary>
    public List<string> SupportedLanguages { get; set; } = new();

    /// <summary>
    /// Model specializations
    /// </summary>
    public List<string> Specializations { get; set; } = new();

    /// <summary>
    /// Whether model supports fine-tuning
    /// </summary>
    public bool SupportsFineTuning { get; set; }

    /// <summary>
    /// Whether model supports instruction following
    /// </summary>
    public bool SupportsInstructionFollowing { get; set; }

    /// <summary>
    /// Whether model supports role-playing
    /// </summary>
    public bool SupportsRolePlaying { get; set; }

    /// <summary>
    /// Whether model supports creative writing
    /// </summary>
    public bool SupportsCreativeWriting { get; set; }

    /// <summary>
    /// Whether model supports analysis
    /// </summary>
    public bool SupportsAnalysis { get; set; }
}

/// <summary>
/// Performance metrics
/// </summary>
public class PerformanceMetrics
{
    /// <summary>
    /// Average response time
    /// </summary>
    public TimeSpan AverageResponseTime { get; set; }

    /// <summary>
    /// Tokens per second
    /// </summary>
    public double TokensPerSecond { get; set; }

    /// <summary>
    /// Memory usage in MB
    /// </summary>
    public double MemoryUsageMB { get; set; }

    /// <summary>
    /// CPU usage percentage
    /// </summary>
    public double CpuUsagePercent { get; set; }

    /// <summary>
    /// Number of successful requests
    /// </summary>
    public int SuccessfulRequests { get; set; }

    /// <summary>
    /// Number of failed requests
    /// </summary>
    public int FailedRequests { get; set; }

    /// <summary>
    /// Success rate
    /// </summary>
    public double SuccessRate => SuccessfulRequests + FailedRequests > 0 
        ? (double)SuccessfulRequests / (SuccessfulRequests + FailedRequests) 
        : 0.0;

    /// <summary>
    /// Cold start time
    /// </summary>
    public TimeSpan ColdStartTime { get; set; }

    /// <summary>
    /// Warm response time
    /// </summary>
    public TimeSpan WarmResponseTime { get; set; }

    /// <summary>
    /// Peak memory usage
    /// </summary>
    public double PeakMemoryUsageMB { get; set; }

    /// <summary>
    /// Peak CPU usage
    /// </summary>
    public double PeakCpuUsagePercent { get; set; }
}

/// <summary>
/// Model configuration
/// </summary>
public class ModelConfiguration
{
    /// <summary>
    /// Temperature setting
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Top-p setting
    /// </summary>
    public double TopP { get; set; } = 0.9;

    /// <summary>
    /// Top-k setting
    /// </summary>
    public int TopK { get; set; } = 40;

    /// <summary>
    /// Repetition penalty
    /// </summary>
    public double RepetitionPenalty { get; set; } = 1.1;

    /// <summary>
    /// Maximum tokens
    /// </summary>
    public int MaxTokens { get; set; } = 2048;

    /// <summary>
    /// Stop sequences
    /// </summary>
    public List<string> StopSequences { get; set; } = new();

    /// <summary>
    /// Whether to stream responses
    /// </summary>
    public bool StreamResponse { get; set; } = true;

    /// <summary>
    /// Context length
    /// </summary>
    public int ContextLength { get; set; } = 4096;

    /// <summary>
    /// Batch size
    /// </summary>
    public int BatchSize { get; set; } = 1;

    /// <summary>
    /// Number of threads
    /// </summary>
    public int Threads { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// GPU layers
    /// </summary>
    public int GpuLayers { get; set; } = 0;

    /// <summary>
    /// Memory usage
    /// </summary>
    public string MemoryUsage { get; set; } = "auto";

    /// <summary>
    /// Custom parameters
    /// </summary>
    public Dictionary<string, object> CustomParameters { get; set; } = new();
}

/// <summary>
/// Model usage statistics
/// </summary>
public class ModelUsage
{
    /// <summary>
    /// Model name
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Total usage count
    /// </summary>
    public int TotalUsageCount { get; set; }

    /// <summary>
    /// Successful usage count
    /// </summary>
    public int SuccessfulUsageCount { get; set; }

    /// <summary>
    /// Failed usage count
    /// </summary>
    public int FailedUsageCount { get; set; }

    /// <summary>
    /// Total tokens generated
    /// </summary>
    public long TotalTokensGenerated { get; set; }

    /// <summary>
    /// Total tokens processed
    /// </summary>
    public long TotalTokensProcessed { get; set; }

    /// <summary>
    /// Total response time
    /// </summary>
    public TimeSpan TotalResponseTime { get; set; }

    /// <summary>
    /// Average response time
    /// </summary>
    public TimeSpan AverageResponseTime => TotalUsageCount > 0 
        ? TimeSpan.FromTicks(TotalResponseTime.Ticks / TotalUsageCount) 
        : TimeSpan.Zero;

    /// <summary>
    /// Last used timestamp
    /// </summary>
    public DateTime LastUsed { get; set; }

    /// <summary>
    /// First used timestamp
    /// </summary>
    public DateTime FirstUsed { get; set; }

    /// <summary>
    /// Usage by task type
    /// </summary>
    public Dictionary<AITaskType, int> UsageByTaskType { get; set; } = new();

    /// <summary>
    /// Usage by time period
    /// </summary>
    public Dictionary<TimePeriod, int> UsageByTimePeriod { get; set; } = new();
}

/// <summary>
/// Model health status
/// </summary>
public enum ModelHealthStatus
{
    Healthy,
    Warning,
    Error,
    Unknown
}

/// <summary>
/// Model requirements
/// </summary>
public class ModelRequirements
{
    /// <summary>
    /// Minimum RAM in MB
    /// </summary>
    public int MinimumRAMMB { get; set; }

    /// <summary>
    /// Recommended RAM in MB
    /// </summary>
    public int RecommendedRAMMB { get; set; }

    /// <summary>
    /// Minimum VRAM in MB
    /// </summary>
    public int MinimumVRAMMB { get; set; }

    /// <summary>
    /// Recommended VRAM in MB
    /// </summary>
    public int RecommendedVRAMMB { get; set; }

    /// <summary>
    /// Minimum CPU cores
    /// </summary>
    public int MinimumCPUCores { get; set; }

    /// <summary>
    /// Recommended CPU cores
    /// </summary>
    public int RecommendedCPUCores { get; set; }

    /// <summary>
    /// Required file system space in MB
    /// </summary>
    public long RequiredDiskSpaceMB { get; set; }

    /// <summary>
    /// Required dependencies
    /// </summary>
    public List<string> RequiredDependencies { get; set; } = new();

    /// <summary>
    /// Supported operating systems
    /// </summary>
    public List<string> SupportedOperatingSystems { get; set; } = new();

    /// <summary>
    /// Supported architectures
    /// </summary>
    public List<string> SupportedArchitectures { get; set; } = new();
}

/// <summary>
/// AI task types
/// </summary>
public enum AITaskType
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
    Summarization
}

/// <summary>
/// Time periods for usage tracking
/// </summary>
public enum TimePeriod
{
    LastHour,
    LastDay,
    LastWeek,
    LastMonth,
    LastYear,
    AllTime
}
