using System;
using System.Collections.Generic;

namespace Genisis.Core.Models;

/// <summary>
/// Setup status
/// </summary>
public class SetupStatus
{
    /// <summary>
    /// Whether setup has been completed
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Whether setup has been skipped
    /// </summary>
    public bool IsSkipped { get; set; }

    /// <summary>
    /// Current setup step
    /// </summary>
    public SetupStep CurrentStep { get; set; }

    /// <summary>
    /// Completed steps
    /// </summary>
    public List<SetupStep> CompletedSteps { get; set; } = new();

    /// <summary>
    /// Setup start time
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Setup completion time
    /// </summary>
    public DateTime? CompletionTime { get; set; }

    /// <summary>
    /// Total setup duration
    /// </summary>
    public TimeSpan? Duration => CompletionTime.HasValue ? CompletionTime.Value - StartTime : null;

    /// <summary>
    /// Setup progress percentage
    /// </summary>
    public double ProgressPercentage { get; set; }

    /// <summary>
    /// Whether setup is in progress
    /// </summary>
    public bool IsInProgress { get; set; }

    /// <summary>
    /// Setup errors
    /// </summary>
    public List<SetupError> Errors { get; set; } = new();

    /// <summary>
    /// Setup warnings
    /// </summary>
    public List<SetupWarning> Warnings { get; set; } = new();
}

/// <summary>
/// Setup progress
/// </summary>
public class SetupProgress
{
    /// <summary>
    /// Current setup step
    /// </summary>
    public SetupStep CurrentStep { get; set; }

    /// <summary>
    /// Completed steps
    /// </summary>
    public List<SetupStep> CompletedSteps { get; set; } = new();

    /// <summary>
    /// Remaining steps
    /// </summary>
    public List<SetupStep> RemainingSteps { get; set; } = new();

    /// <summary>
    /// Progress percentage
    /// </summary>
    public double ProgressPercentage { get; set; }

    /// <summary>
    /// Whether current step can be skipped
    /// </summary>
    public bool CanSkip { get; set; }

    /// <summary>
    /// Whether user can go back
    /// </summary>
    public bool CanGoBack { get; set; }

    /// <summary>
    /// Current step data
    /// </summary>
    public SetupStepData CurrentStepData { get; set; } = new();

    /// <summary>
    /// Setup status
    /// </summary>
    public SetupStatus Status { get; set; } = new();
}

/// <summary>
/// Setup steps
/// </summary>
public enum SetupStep
{
    Welcome,
    ModelDiscovery,
    ModelSelection,
    PerformanceTesting,
    Configuration,
    Tutorial,
    Completion
}

/// <summary>
/// Setup step data
/// </summary>
public class SetupStepData
{
    /// <summary>
    /// Setup step
    /// </summary>
    public SetupStep Step { get; set; }

    /// <summary>
    /// Step title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Step description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Step instructions
    /// </summary>
    public string Instructions { get; set; } = string.Empty;

    /// <summary>
    /// Step options
    /// </summary>
    public List<SetupOption> Options { get; set; } = new();

    /// <summary>
    /// Whether step is required
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Estimated duration
    /// </summary>
    public TimeSpan EstimatedDuration { get; set; }

    /// <summary>
    /// Step data
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>
    /// Step validation rules
    /// </summary>
    public List<SetupValidationRule> ValidationRules { get; set; } = new();

    /// <summary>
    /// Step dependencies
    /// </summary>
    public List<SetupStep> Dependencies { get; set; } = new();

    /// <summary>
    /// Step completion criteria
    /// </summary>
    public List<SetupCompletionCriterion> CompletionCriteria { get; set; } = new();
}

/// <summary>
/// Setup option
/// </summary>
public class SetupOption
{
    /// <summary>
    /// Option ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Option title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Option description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Option value
    /// </summary>
    public object Value { get; set; } = string.Empty;

    /// <summary>
    /// Whether option is selected
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Whether option is recommended
    /// </summary>
    public bool IsRecommended { get; set; }

    /// <summary>
    /// Option icon
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Option category
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Option dependencies
    /// </summary>
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// Option conflicts
    /// </summary>
    public List<string> Conflicts { get; set; } = new();
}

/// <summary>
/// Setup validation rule
/// </summary>
public class SetupValidationRule
{
    /// <summary>
    /// Rule name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Rule description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Validation function
    /// </summary>
    public Func<SetupStepData, bool> Validator { get; set; } = _ => true;

    /// <summary>
    /// Error message
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Whether rule is required
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Rule priority
    /// </summary>
    public int Priority { get; set; }
}

/// <summary>
/// Setup completion criterion
/// </summary>
public class SetupCompletionCriterion
{
    /// <summary>
    /// Criterion name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Criterion description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Completion function
    /// </summary>
    public Func<SetupStepData, bool> CompletionChecker { get; set; } = _ => true;

    /// <summary>
    /// Whether criterion is required
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Criterion weight
    /// </summary>
    public double Weight { get; set; } = 1.0;
}

/// <summary>
/// Setup validation result
/// </summary>
public class SetupValidationResult
{
    /// <summary>
    /// Whether validation passed
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Validation errors
    /// </summary>
    public List<SetupError> Errors { get; set; } = new();

    /// <summary>
    /// Validation warnings
    /// </summary>
    public List<SetupWarning> Warnings { get; set; } = new();

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
/// Setup error
/// </summary>
public class SetupError
{
    /// <summary>
    /// Error code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Error details
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Error severity
    /// </summary>
    public SetupErrorSeverity Severity { get; set; }

    /// <summary>
    /// Error timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Related setup step
    /// </summary>
    public SetupStep? RelatedStep { get; set; }

    /// <summary>
    /// Suggested resolution
    /// </summary>
    public string SuggestedResolution { get; set; } = string.Empty;
}

/// <summary>
/// Setup warning
/// </summary>
public class SetupWarning
{
    /// <summary>
    /// Warning code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Warning message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Warning details
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Warning severity
    /// </summary>
    public SetupWarningSeverity Severity { get; set; }

    /// <summary>
    /// Warning timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Related setup step
    /// </summary>
    public SetupStep? RelatedStep { get; set; }

    /// <summary>
    /// Suggested action
    /// </summary>
    public string SuggestedAction { get; set; } = string.Empty;
}

/// <summary>
/// Setup error severity
/// </summary>
public enum SetupErrorSeverity
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Setup warning severity
/// </summary>
public enum SetupWarningSeverity
{
    Info,
    Warning,
    Important
}

/// <summary>
/// Setup configuration
/// </summary>
public class SetupConfiguration
{
    /// <summary>
    /// Whether setup is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Whether setup can be skipped
    /// </summary>
    public bool CanSkip { get; set; } = true;

    /// <summary>
    /// Whether setup can be reset
    /// </summary>
    public bool CanReset { get; set; } = true;

    /// <summary>
    /// Setup timeout
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Setup steps
    /// </summary>
    public List<SetupStep> Steps { get; set; } = new();

    /// <summary>
    /// Required steps
    /// </summary>
    public List<SetupStep> RequiredSteps { get; set; } = new();

    /// <summary>
    /// Optional steps
    /// </summary>
    public List<SetupStep> OptionalSteps { get; set; } = new();

    /// <summary>
    /// Setup preferences
    /// </summary>
    public SetupPreferences Preferences { get; set; } = new();
}

/// <summary>
/// Setup preferences
/// </summary>
public class SetupPreferences
{
    /// <summary>
    /// Preferred language
    /// </summary>
    public string Language { get; set; } = "en-US";

    /// <summary>
    /// Preferred theme
    /// </summary>
    public string Theme { get; set; } = "default";

    /// <summary>
    /// Whether to show advanced options
    /// </summary>
    public bool ShowAdvancedOptions { get; set; } = false;

    /// <summary>
    /// Whether to enable analytics
    /// </summary>
    public bool EnableAnalytics { get; set; } = true;

    /// <summary>
    /// Whether to enable error reporting
    /// </summary>
    public bool EnableErrorReporting { get; set; } = true;

    /// <summary>
    /// Whether to enable automatic updates
    /// </summary>
    public bool EnableAutomaticUpdates { get; set; } = true;

    /// <summary>
    /// Custom preferences
    /// </summary>
    public Dictionary<string, object> CustomPreferences { get; set; } = new();
}
