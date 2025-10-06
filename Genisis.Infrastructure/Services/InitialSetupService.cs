using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Genisis.Core.Models;
using Genisis.Core.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Genisis.Infrastructure.Services;

/// <summary>
/// Initial setup service implementation
/// </summary>
public class InitialSetupService : IInitialSetupService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<InitialSetupService> _logger;
    private readonly ILocalModelManager _modelManager;
    private readonly string _setupStatusCacheKey = "setup_status";
    private readonly string _setupDataCacheKey = "setup_data";
    private readonly string _setupConfigPath = "setup_config.json";

    public InitialSetupService(
        IMemoryCache cache,
        ILogger<InitialSetupService> logger,
        ILocalModelManager modelManager)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _modelManager = modelManager ?? throw new ArgumentNullException(nameof(modelManager));
    }

    public async Task<SetupStatus> GetSetupStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Check cache first
            if (_cache.TryGetValue(_setupStatusCacheKey, out SetupStatus? cachedStatus))
            {
                return cachedStatus ?? CreateDefaultSetupStatus();
            }

            // Load from file system
            var status = await LoadSetupStatusFromFileAsync(cancellationToken);
            if (status != null)
            {
                _cache.Set(_setupStatusCacheKey, status, TimeSpan.FromHours(1));
                return status;
            }

            // Return default status
            return CreateDefaultSetupStatus();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get setup status");
            return CreateDefaultSetupStatus();
        }
    }

    public async Task<SetupProgress> StartSetupAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting initial setup process");

            var status = await GetSetupStatusAsync(cancellationToken);
            status.IsInProgress = true;
            status.StartTime = DateTime.UtcNow;
            status.CurrentStep = SetupStep.Welcome;
            status.CompletedSteps.Clear();
            status.Errors.Clear();
            status.Warnings.Clear();

            await SaveSetupStatusAsync(status, cancellationToken);

            var progress = await CreateSetupProgressAsync(status, cancellationToken);
            _logger.LogInformation("Setup started at step {Step}", status.CurrentStep);

            return progress;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start setup");
            throw;
        }
    }

    public async Task<SetupProgress> ContinueSetupAsync(SetupStep step, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Continuing setup to step {Step}", step);

            var status = await GetSetupStatusAsync(cancellationToken);
            if (!status.IsInProgress)
            {
                throw new InvalidOperationException("Setup is not in progress");
            }

            // Validate step transition
            if (!IsValidStepTransition(status.CurrentStep, step))
            {
                throw new InvalidOperationException($"Invalid step transition from {status.CurrentStep} to {step}");
            }

            // Complete current step if not already completed
            if (!status.CompletedSteps.Contains(status.CurrentStep))
            {
                status.CompletedSteps.Add(status.CurrentStep);
            }

            // Move to next step
            status.CurrentStep = step;
            status.ProgressPercentage = CalculateProgressPercentage(status);

            await SaveSetupStatusAsync(status, cancellationToken);

            var progress = await CreateSetupProgressAsync(status, cancellationToken);
            _logger.LogInformation("Setup continued to step {Step}", step);

            return progress;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to continue setup to step {Step}", step);
            throw;
        }
    }

    public async Task CompleteSetupAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Completing setup process");

            var status = await GetSetupStatusAsync(cancellationToken);
            status.IsCompleted = true;
            status.IsInProgress = false;
            status.CompletionTime = DateTime.UtcNow;
            status.ProgressPercentage = 100.0;

            // Complete remaining steps
            var allSteps = Enum.GetValues<SetupStep>().ToList();
            foreach (var step in allSteps.Except(status.CompletedSteps))
            {
                status.CompletedSteps.Add(step);
            }

            await SaveSetupStatusAsync(status, cancellationToken);
            _logger.LogInformation("Setup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete setup");
            throw;
        }
    }

    public async Task SkipSetupAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Skipping setup process");

            var status = await GetSetupStatusAsync(cancellationToken);
            status.IsSkipped = true;
            status.IsInProgress = false;
            status.CompletionTime = DateTime.UtcNow;

            await SaveSetupStatusAsync(status, cancellationToken);
            _logger.LogInformation("Setup skipped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to skip setup");
            throw;
        }
    }

    public async Task ResetSetupAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Resetting setup process");

            var status = CreateDefaultSetupStatus();
            await SaveSetupStatusAsync(status, cancellationToken);

            // Clear cache
            _cache.Remove(_setupStatusCacheKey);
            _cache.Remove(_setupDataCacheKey);

            _logger.LogInformation("Setup reset successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset setup");
            throw;
        }
    }

    public async Task<SetupStepData> GetSetupStepDataAsync(SetupStep step, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting setup step data for {Step}", step);

            var stepData = step switch
            {
                SetupStep.Welcome => CreateWelcomeStepData(),
                SetupStep.ModelDiscovery => await CreateModelDiscoveryStepDataAsync(cancellationToken),
                SetupStep.ModelSelection => await CreateModelSelectionStepDataAsync(cancellationToken),
                SetupStep.PerformanceTesting => await CreatePerformanceTestingStepDataAsync(cancellationToken),
                SetupStep.Configuration => await CreateConfigurationStepDataAsync(cancellationToken),
                SetupStep.Tutorial => CreateTutorialStepData(),
                SetupStep.Completion => CreateCompletionStepData(),
                _ => throw new ArgumentException($"Unknown setup step: {step}")
            };

            return stepData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get setup step data for {Step}", step);
            throw;
        }
    }

    public async Task<SetupValidationResult> ValidateSetupStepAsync(SetupStep step, SetupStepData data, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Validating setup step {Step}", step);

            var result = new SetupValidationResult { IsValid = true };

            // Run validation rules
            foreach (var rule in data.ValidationRules)
            {
                try
                {
                    var isValid = rule.Validator(data);
                    if (!isValid)
                    {
                        result.IsValid = false;
                        result.Errors.Add(new SetupError
                        {
                            Code = $"VALIDATION_{rule.Name}",
                            Message = rule.ErrorMessage,
                            Severity = rule.IsRequired ? SetupErrorSeverity.High : SetupErrorSeverity.Medium,
                            RelatedStep = step
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Validation rule {RuleName} failed", rule.Name);
                    result.Warnings.Add(new SetupWarning
                    {
                        Code = $"VALIDATION_ERROR_{rule.Name}",
                        Message = $"Validation rule {rule.Name} failed: {ex.Message}",
                        Severity = SetupWarningSeverity.Warning,
                        RelatedStep = step
                    });
                }
            }

            // Calculate validation score
            result.Score = CalculateValidationScore(result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate setup step {Step}", step);
            throw;
        }
    }

    public async Task SaveSetupStepDataAsync(SetupStep step, SetupStepData data, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Saving setup step data for {Step}", step);

            var cacheKey = $"{_setupDataCacheKey}_{step}";
            _cache.Set(cacheKey, data, TimeSpan.FromHours(1));

            // TODO: Persist to file system or database
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save setup step data for {Step}", step);
            throw;
        }
    }

    private SetupStatus CreateDefaultSetupStatus()
    {
        return new SetupStatus
        {
            IsCompleted = false,
            IsSkipped = false,
            CurrentStep = SetupStep.Welcome,
            CompletedSteps = new List<SetupStep>(),
            StartTime = DateTime.UtcNow,
            ProgressPercentage = 0.0,
            IsInProgress = false,
            Errors = new List<SetupError>(),
            Warnings = new List<SetupWarning>()
        };
    }

    private async Task<SetupStatus?> LoadSetupStatusFromFileAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(_setupConfigPath))
                return null;

            var json = await File.ReadAllTextAsync(_setupConfigPath, cancellationToken);
            var status = JsonSerializer.Deserialize<SetupStatus>(json);
            return status;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load setup status from file");
            return null;
        }
    }

    private async Task SaveSetupStatusAsync(SetupStatus status, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(status, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_setupConfigPath, json, cancellationToken);

            _cache.Set(_setupStatusCacheKey, status, TimeSpan.FromHours(1));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save setup status");
            throw;
        }
    }

    private async Task<SetupProgress> CreateSetupProgressAsync(SetupStatus status, CancellationToken cancellationToken)
    {
        var allSteps = Enum.GetValues<SetupStep>().ToList();
        var remainingSteps = allSteps.SkipWhile(s => s != status.CurrentStep).ToList();

        var progress = new SetupProgress
        {
            CurrentStep = status.CurrentStep,
            CompletedSteps = status.CompletedSteps.ToList(),
            RemainingSteps = remainingSteps,
            ProgressPercentage = status.ProgressPercentage,
            CanSkip = CanSkipStep(status.CurrentStep),
            CanGoBack = status.CompletedSteps.Any(),
            Status = status
        };

        // Get current step data
        progress.CurrentStepData = await GetSetupStepDataAsync(status.CurrentStep, cancellationToken);

        return progress;
    }

    private bool IsValidStepTransition(SetupStep from, SetupStep to)
    {
        var allSteps = Enum.GetValues<SetupStep>().ToList();
        var fromIndex = allSteps.IndexOf(from);
        var toIndex = allSteps.IndexOf(to);

        // Allow forward movement or staying at same step
        return toIndex >= fromIndex;
    }

    private double CalculateProgressPercentage(SetupStatus status)
    {
        var allSteps = Enum.GetValues<SetupStep>().ToList();
        var totalSteps = allSteps.Count;
        var completedSteps = status.CompletedSteps.Count;

        return (double)completedSteps / totalSteps * 100.0;
    }

    private bool CanSkipStep(SetupStep step)
    {
        return step switch
        {
            SetupStep.Welcome => false,
            SetupStep.ModelDiscovery => false,
            SetupStep.ModelSelection => true,
            SetupStep.PerformanceTesting => true,
            SetupStep.Configuration => true,
            SetupStep.Tutorial => true,
            SetupStep.Completion => false,
            _ => true
        };
    }

    private SetupStepData CreateWelcomeStepData()
    {
        return new SetupStepData
        {
            Step = SetupStep.Welcome,
            Title = "Welcome to World Builder",
            Description = "Let's set up your AI-powered story creation environment",
            Instructions = "This setup will help you configure local AI models and optimize your writing experience.",
            IsRequired = true,
            EstimatedDuration = TimeSpan.FromMinutes(2),
            Options = new List<SetupOption>
            {
                new SetupOption
                {
                    Id = "continue",
                    Title = "Continue Setup",
                    Description = "Proceed with the setup process",
                    Value = true,
                    IsSelected = true,
                    IsRecommended = true
                },
                new SetupOption
                {
                    Id = "skip",
                    Title = "Skip Setup",
                    Description = "Skip setup and use default settings",
                    Value = false,
                    IsSelected = false
                }
            }
        };
    }

    private async Task<SetupStepData> CreateModelDiscoveryStepDataAsync(CancellationToken cancellationToken)
    {
        var models = await _modelManager.DiscoverModelsAsync(cancellationToken);
        var modelOptions = models.Select(m => new SetupOption
        {
            Id = m.Name,
            Title = m.DisplayName,
            Description = m.Description,
            Value = m,
            IsSelected = m.IsRecommended,
            IsRecommended = m.IsRecommended,
            Icon = GetModelIcon(m.Type)
        }).ToList();

        return new SetupStepData
        {
            Step = SetupStep.ModelDiscovery,
            Title = "Discover AI Models",
            Description = "Scanning for available local AI models",
            Instructions = "We're looking for AI models installed on your system. This may take a few moments.",
            IsRequired = true,
            EstimatedDuration = TimeSpan.FromMinutes(5),
            Options = modelOptions,
            Data = new Dictionary<string, object>
            {
                ["discoveredModels"] = models.ToList(),
                ["discoveryInProgress"] = true
            }
        };
    }

    private async Task<SetupStepData> CreateModelSelectionStepDataAsync(CancellationToken cancellationToken)
    {
        var models = await _modelManager.DiscoverModelsAsync(cancellationToken);
        var recommendedModels = models.Where(m => m.IsRecommended).ToList();

        return new SetupStepData
        {
            Step = SetupStep.ModelSelection,
            Title = "Select AI Models",
            Description = "Choose which AI models to use for different tasks",
            Instructions = "Select the models you want to use for creative writing, analysis, and other tasks.",
            IsRequired = false,
            EstimatedDuration = TimeSpan.FromMinutes(3),
            Options = models.Select(m => new SetupOption
            {
                Id = m.Name,
                Title = m.DisplayName,
                Description = $"{m.Description} ({FormatFileSize(m.SizeBytes)})",
                Value = m,
                IsSelected = recommendedModels.Contains(m),
                IsRecommended = m.IsRecommended,
                Icon = GetModelIcon(m.Type),
                Category = m.Type.ToString()
            }).ToList()
        };
    }

    private async Task<SetupStepData> CreatePerformanceTestingStepDataAsync(CancellationToken cancellationToken)
    {
        return new SetupStepData
        {
            Step = SetupStep.PerformanceTesting,
            Title = "Performance Testing",
            Description = "Test the performance of your selected models",
            Instructions = "We'll run some tests to determine the best settings for your hardware.",
            IsRequired = false,
            EstimatedDuration = TimeSpan.FromMinutes(10),
            Options = new List<SetupOption>
            {
                new SetupOption
                {
                    Id = "run_tests",
                    Title = "Run Performance Tests",
                    Description = "Test model performance and optimize settings",
                    Value = true,
                    IsSelected = true,
                    IsRecommended = true
                },
                new SetupOption
                {
                    Id = "skip_tests",
                    Title = "Skip Tests",
                    Description = "Use default settings without testing",
                    Value = false,
                    IsSelected = false
                }
            }
        };
    }

    private async Task<SetupStepData> CreateConfigurationStepDataAsync(CancellationToken cancellationToken)
    {
        return new SetupStepData
        {
            Step = SetupStep.Configuration,
            Title = "Configuration",
            Description = "Configure your AI settings and preferences",
            Instructions = "Set up your preferred AI behavior, response styles, and other preferences.",
            IsRequired = false,
            EstimatedDuration = TimeSpan.FromMinutes(5),
            Options = new List<SetupOption>
            {
                new SetupOption
                {
                    Id = "creative_mode",
                    Title = "Creative Mode",
                    Description = "Optimize for creative writing and storytelling",
                    Value = "creative",
                    IsSelected = true,
                    IsRecommended = true,
                    Category = "Writing Style"
                },
                new SetupOption
                {
                    Id = "analytical_mode",
                    Title = "Analytical Mode",
                    Description = "Optimize for analysis and critical thinking",
                    Value = "analytical",
                    IsSelected = false,
                    Category = "Writing Style"
                },
                new SetupOption
                {
                    Id = "balanced_mode",
                    Title = "Balanced Mode",
                    Description = "Balance between creativity and analysis",
                    Value = "balanced",
                    IsSelected = false,
                    Category = "Writing Style"
                }
            }
        };
    }

    private SetupStepData CreateTutorialStepData()
    {
        return new SetupStepData
        {
            Step = SetupStep.Tutorial,
            Title = "Quick Tutorial",
            Description = "Learn the basics of using World Builder",
            Instructions = "Take a quick tour of the main features and learn how to get started.",
            IsRequired = false,
            EstimatedDuration = TimeSpan.FromMinutes(5),
            Options = new List<SetupOption>
            {
                new SetupOption
                {
                    Id = "start_tutorial",
                    Title = "Start Tutorial",
                    Description = "Take the interactive tutorial",
                    Value = true,
                    IsSelected = true,
                    IsRecommended = true
                },
                new SetupOption
                {
                    Id = "skip_tutorial",
                    Title = "Skip Tutorial",
                    Description = "Skip the tutorial and start using the app",
                    Value = false,
                    IsSelected = false
                }
            }
        };
    }

    private SetupStepData CreateCompletionStepData()
    {
        return new SetupStepData
        {
            Step = SetupStep.Completion,
            Title = "Setup Complete",
            Description = "Your World Builder is ready to use",
            Instructions = "Setup is complete! You can now start creating your stories with AI assistance.",
            IsRequired = true,
            EstimatedDuration = TimeSpan.FromMinutes(1),
            Options = new List<SetupOption>
            {
                new SetupOption
                {
                    Id = "finish",
                    Title = "Finish Setup",
                    Description = "Complete setup and start using World Builder",
                    Value = true,
                    IsSelected = true,
                    IsRecommended = true
                }
            }
        };
    }

    private string GetModelIcon(ModelType modelType)
    {
        return modelType switch
        {
            ModelType.TextGeneration => "📝",
            ModelType.CodeGeneration => "💻",
            ModelType.CreativeWriting => "✨",
            ModelType.Analysis => "🔍",
            ModelType.Conversation => "💬",
            ModelType.Multimodal => "🎨",
            ModelType.Specialized => "⚙️",
            ModelType.General => "🤖",
            _ => "❓"
        };
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private double CalculateValidationScore(SetupValidationResult result)
    {
        if (result.Errors.Any(e => e.Severity == SetupErrorSeverity.Critical))
            return 0.0;

        var errorPenalty = result.Errors.Sum(e => e.Severity switch
        {
            SetupErrorSeverity.High => 0.3,
            SetupErrorSeverity.Medium => 0.2,
            SetupErrorSeverity.Low => 0.1,
            _ => 0.0
        });

        var warningPenalty = result.Warnings.Sum(w => w.Severity switch
        {
            SetupWarningSeverity.Important => 0.1,
            SetupWarningSeverity.Warning => 0.05,
            SetupWarningSeverity.Info => 0.02,
            _ => 0.0
        });

        return Math.Max(0.0, 1.0 - errorPenalty - warningPenalty);
    }
}
