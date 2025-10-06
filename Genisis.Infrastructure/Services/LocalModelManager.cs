using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Genisis.Core.Models;
using Genisis.Core.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Genisis.Infrastructure.Services;

/// <summary>
/// Local AI model manager implementation
/// </summary>
public class LocalModelManager : ILocalModelManager
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LocalModelManager> _logger;
    private readonly string _modelsCacheKey = "local_models";
    private readonly string _modelConfigsCacheKey = "model_configs";
    private readonly string _modelUsageCacheKey = "model_usage";

    public LocalModelManager(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<LocalModelManager> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<LocalModel>> DiscoverModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Check cache first
            if (_cache.TryGetValue(_modelsCacheKey, out List<LocalModel>? cachedModels))
            {
                _logger.LogDebug("Returning cached models");
                return cachedModels ?? new List<LocalModel>();
            }

            _logger.LogInformation("Discovering local AI models");

            var models = new List<LocalModel>();

            // Discover Ollama models
            var ollamaModels = await DiscoverOllamaModelsAsync(cancellationToken);
            models.AddRange(ollamaModels);

            // Discover other local model providers
            var otherModels = await DiscoverOtherModelsAsync(cancellationToken);
            models.AddRange(otherModels);

            // Cache discovered models
            _cache.Set(_modelsCacheKey, models, TimeSpan.FromMinutes(30));

            _logger.LogInformation("Discovered {Count} local models", models.Count);
            return models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discover local models");
            return new List<LocalModel>();
        }
    }

    public async Task<LocalModel?> GetModelAsync(string modelName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            return null;

        try
        {
            var models = await DiscoverModelsAsync(cancellationToken);
            return models.FirstOrDefault(m => m.Name.Equals(modelName, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get model {ModelName}", modelName);
            return null;
        }
    }

    public async Task<bool> ValidateModelAsync(string modelName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            return false;

        try
        {
            _logger.LogDebug("Validating model {ModelName}", modelName);

            // Test model with a simple prompt
            var testPrompt = "Hello, please respond with 'OK' to confirm you are working.";
            var response = new StringBuilder();
            var timeout = TimeSpan.FromSeconds(30);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            await foreach (var chunk in StreamCompletionAsync(modelName, testPrompt, cts.Token))
            {
                response.Append(chunk);
                if (response.Length > 100) break; // Limit response length for validation
            }

            var isValid = response.ToString().Contains("OK", StringComparison.OrdinalIgnoreCase);
            _logger.LogDebug("Model {ModelName} validation result: {IsValid}", modelName, isValid);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Model {ModelName} validation failed", modelName);
            return false;
        }
    }

    public async Task<ModelPerformanceProfile> GetPerformanceProfileAsync(string modelName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            return new ModelPerformanceProfile();

        try
        {
            _logger.LogDebug("Getting performance profile for model {ModelName}", modelName);

            var profile = new ModelPerformanceProfile
            {
                ModelName = modelName,
                TestResults = new List<PerformanceTest>()
            };

            // Run performance tests
            var tests = new[]
            {
                new PerformanceTest { Name = "Short Response", Prompt = "Say hello", ExpectedTokens = 10 },
                new PerformanceTest { Name = "Medium Response", Prompt = "Write a short story about a cat", ExpectedTokens = 100 },
                new PerformanceTest { Name = "Long Response", Prompt = "Write a detailed analysis of character development in literature", ExpectedTokens = 500 }
            };

            foreach (var test in tests)
            {
                var testResult = await RunPerformanceTestAsync(modelName, test, cancellationToken);
                profile.TestResults.Add(testResult);
            }

            // Calculate overall metrics
            profile.ColdStartTime = profile.TestResults.FirstOrDefault()?.ResponseTime ?? TimeSpan.Zero;
            profile.WarmResponseTime = profile.TestResults.Skip(1).FirstOrDefault()?.ResponseTime ?? TimeSpan.Zero;
            profile.TokensPerSecond = CalculateTokensPerSecond(profile.TestResults);
            profile.MemoryPeakMB = await GetModelMemoryUsageAsync(modelName, cancellationToken);
            profile.CpuPeakPercent = await GetModelCpuUsageAsync(modelName, cancellationToken);

            _logger.LogDebug("Performance profile completed for model {ModelName}", modelName);
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get performance profile for model {ModelName}", modelName);
            return new ModelPerformanceProfile { ModelName = modelName };
        }
    }

    public async Task<IEnumerable<LocalModel>> GetRecommendedModelsAsync(AITaskType taskType, CancellationToken cancellationToken = default)
    {
        try
        {
            var models = await DiscoverModelsAsync(cancellationToken);
            var recommendations = new List<LocalModel>();

            foreach (var model in models)
            {
                var score = CalculateRecommendationScore(model, taskType);
                if (score > 0.5) // Only recommend models with score > 0.5
                {
                    model.IsRecommended = true;
                    recommendations.Add(model);
                }
            }

            return recommendations.OrderByDescending(m => CalculateRecommendationScore(m, taskType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recommended models for task type {TaskType}", taskType);
            return new List<LocalModel>();
        }
    }

    public async Task<ModelConfiguration> GetModelConfigurationAsync(string modelName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            return new ModelConfiguration();

        try
        {
            var cacheKey = $"{_modelConfigsCacheKey}_{modelName}";
            if (_cache.TryGetValue(cacheKey, out ModelConfiguration? cachedConfig))
            {
                return cachedConfig ?? new ModelConfiguration();
            }

            // Get default configuration for model
            var config = await GetDefaultModelConfigurationAsync(modelName, cancellationToken);

            // Cache configuration
            _cache.Set(cacheKey, config, TimeSpan.FromHours(1));

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get configuration for model {ModelName}", modelName);
            return new ModelConfiguration();
        }
    }

    public async Task SaveModelConfigurationAsync(string modelName, ModelConfiguration config, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modelName) || config == null)
            return;

        try
        {
            var cacheKey = $"{_modelConfigsCacheKey}_{modelName}";
            _cache.Set(cacheKey, config, TimeSpan.FromHours(1));

            // TODO: Persist to file system or database
            _logger.LogDebug("Saved configuration for model {ModelName}", modelName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration for model {ModelName}", modelName);
        }
    }

    public async Task UpdateModelUsageAsync(string modelName, ModelUsage usage, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modelName) || usage == null)
            return;

        try
        {
            var cacheKey = $"{_modelUsageCacheKey}_{modelName}";
            _cache.Set(cacheKey, usage, TimeSpan.FromDays(30));

            // TODO: Persist to file system or database
            _logger.LogDebug("Updated usage statistics for model {ModelName}", modelName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update usage statistics for model {ModelName}", modelName);
        }
    }

    public async Task<ModelUsage> GetModelUsageAsync(string modelName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            return new ModelUsage { ModelName = modelName };

        try
        {
            var cacheKey = $"{_modelUsageCacheKey}_{modelName}";
            if (_cache.TryGetValue(cacheKey, out ModelUsage? cachedUsage))
            {
                return cachedUsage ?? new ModelUsage { ModelName = modelName };
            }

            // Return default usage statistics
            return new ModelUsage { ModelName = modelName };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get usage statistics for model {ModelName}", modelName);
            return new ModelUsage { ModelName = modelName };
        }
    }

    public async Task<IEnumerable<ModelUsage>> GetAllModelUsageAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var models = await DiscoverModelsAsync(cancellationToken);
            var usageStats = new List<ModelUsage>();

            foreach (var model in models)
            {
                var usage = await GetModelUsageAsync(model.Name, cancellationToken);
                usageStats.Add(usage);
            }

            return usageStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all model usage statistics");
            return new List<ModelUsage>();
        }
    }

    private async Task<List<LocalModel>> DiscoverOllamaModelsAsync(CancellationToken cancellationToken)
    {
        var models = new List<LocalModel>();

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var response = await _httpClient.GetAsync("/api/tags", cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Ollama API not available");
                return models;
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cts.Token);
            if (json.TryGetProperty("models", out var modelsArray))
            {
                foreach (var modelElement in modelsArray.EnumerateArray())
                {
                    var model = await CreateOllamaModelAsync(modelElement, cancellationToken);
                    if (model != null)
                    {
                        models.Add(model);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to discover Ollama models");
        }

        return models;
    }

    private async Task<List<LocalModel>> DiscoverOtherModelsAsync(CancellationToken cancellationToken)
    {
        var models = new List<LocalModel>();

        // TODO: Implement discovery for other local model providers
        // - Transformers.js models
        // - ONNX models
        // - Custom model formats

        return models;
    }

    private async Task<LocalModel?> CreateOllamaModelAsync(JsonElement modelElement, CancellationToken cancellationToken)
    {
        try
        {
            var name = modelElement.GetProperty("name").GetString() ?? "";
            var size = modelElement.TryGetProperty("size", out var sizeElement) ? sizeElement.GetInt64() : 0;
            var modifiedAt = modelElement.TryGetProperty("modified_at", out var modifiedElement) 
                ? DateTimeOffset.FromUnixTimeSeconds(modifiedElement.GetInt64()).DateTime 
                : DateTime.MinValue;

            var model = new LocalModel
            {
                Name = name,
                DisplayName = GetDisplayName(name),
                SizeBytes = size,
                Type = DetermineModelType(name),
                Capabilities = await GetModelCapabilitiesAsync(name, cancellationToken),
                Performance = new PerformanceMetrics(),
                IsInstalled = true,
                IsRecommended = IsRecommendedModel(name),
                LastUsed = modifiedAt,
                UsageCount = 0,
                Description = GetModelDescription(name),
                Version = GetModelVersion(name),
                Tags = GetModelTags(name),
                Configuration = await GetDefaultModelConfigurationAsync(name, cancellationToken),
                IsAvailable = true,
                HealthStatus = ModelHealthStatus.Unknown,
                LastHealthCheck = DateTime.MinValue,
                InstallationPath = $"ollama:{name}",
                Dependencies = new List<string>(),
                Requirements = GetModelRequirements(name)
            };

            return model;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create Ollama model from JSON element");
            return null;
        }
    }

    private string GetDisplayName(string modelName)
    {
        // Convert technical model names to user-friendly display names
        return modelName.Replace(":", " - ").Replace("_", " ");
    }

    private ModelType DetermineModelType(string modelName)
    {
        var lowerName = modelName.ToLowerInvariant();
        
        if (lowerName.Contains("code") || lowerName.Contains("coder"))
            return ModelType.CodeGeneration;
        if (lowerName.Contains("creative") || lowerName.Contains("story"))
            return ModelType.CreativeWriting;
        if (lowerName.Contains("analysis") || lowerName.Contains("analyst"))
            return ModelType.Analysis;
        if (lowerName.Contains("multimodal") || lowerName.Contains("vision"))
            return ModelType.Multimodal;
        
        return ModelType.TextGeneration;
    }

    private async Task<ModelCapabilities> GetModelCapabilitiesAsync(string modelName, CancellationToken cancellationToken)
    {
        // TODO: Implement actual capability detection
        return new ModelCapabilities
        {
            MaxContextLength = 4096,
            SupportsStreaming = true,
            SupportsFunctionCalling = false,
            SupportsMultimodal = false,
            SupportedLanguages = new List<string> { "English" },
            Specializations = new List<string>(),
            SupportsFineTuning = false,
            SupportsInstructionFollowing = true,
            SupportsRolePlaying = true,
            SupportsCreativeWriting = true,
            SupportsAnalysis = true
        };
    }

    private bool IsRecommendedModel(string modelName)
    {
        var lowerName = modelName.ToLowerInvariant();
        var recommendedModels = new[]
        {
            "llama2", "llama3", "mistral", "codellama", "phi", "gemma", "qwen"
        };

        return recommendedModels.Any(rec => lowerName.Contains(rec));
    }

    private string GetModelDescription(string modelName)
    {
        var lowerName = modelName.ToLowerInvariant();
        
        if (lowerName.Contains("llama"))
            return "Meta's Llama model for general text generation";
        if (lowerName.Contains("mistral"))
            return "Mistral AI's efficient language model";
        if (lowerName.Contains("code"))
            return "Specialized model for code generation and analysis";
        if (lowerName.Contains("phi"))
            return "Microsoft's Phi model for efficient text generation";
        
        return "Local AI model for text generation";
    }

    private string GetModelVersion(string modelName)
    {
        // Extract version from model name
        var parts = modelName.Split(':');
        return parts.Length > 1 ? parts[1] : "latest";
    }

    private List<string> GetModelTags(string modelName)
    {
        var tags = new List<string>();
        var lowerName = modelName.ToLowerInvariant();

        if (lowerName.Contains("llama")) tags.Add("llama");
        if (lowerName.Contains("mistral")) tags.Add("mistral");
        if (lowerName.Contains("code")) tags.Add("code");
        if (lowerName.Contains("creative")) tags.Add("creative");
        if (lowerName.Contains("analysis")) tags.Add("analysis");

        return tags;
    }

    private async Task<ModelConfiguration> GetDefaultModelConfigurationAsync(string modelName, CancellationToken cancellationToken)
    {
        // TODO: Implement model-specific default configurations
        return new ModelConfiguration();
    }

    private ModelRequirements GetModelRequirements(string modelName)
    {
        // TODO: Implement actual requirement detection
        return new ModelRequirements
        {
            MinimumRAMMB = 2048,
            RecommendedRAMMB = 4096,
            MinimumVRAMMB = 0,
            RecommendedVRAMMB = 0,
            MinimumCPUCores = 2,
            RecommendedCPUCores = 4,
            RequiredDiskSpaceMB = 1000,
            RequiredDependencies = new List<string>(),
            SupportedOperatingSystems = new List<string> { "Windows", "Linux", "macOS" },
            SupportedArchitectures = new List<string> { "x64", "ARM64" }
        };
    }

    private double CalculateRecommendationScore(LocalModel model, AITaskType taskType)
    {
        var score = 0.0;

        // Base score from model type
        switch (taskType)
        {
            case AITaskType.CodeGeneration:
                if (model.Type == ModelType.CodeGeneration) score += 0.8;
                else if (model.Capabilities.Specializations.Contains("code")) score += 0.6;
                break;
            case AITaskType.CreativeWriting:
                if (model.Type == ModelType.CreativeWriting) score += 0.8;
                else if (model.Capabilities.SupportsCreativeWriting) score += 0.6;
                break;
            case AITaskType.Analysis:
                if (model.Type == ModelType.Analysis) score += 0.8;
                else if (model.Capabilities.SupportsAnalysis) score += 0.6;
                break;
            default:
                if (model.Type == ModelType.TextGeneration || model.Type == ModelType.General) score += 0.7;
                break;
        }

        // Boost score for recommended models
        if (model.IsRecommended) score += 0.2;

        // Boost score for good performance
        if (model.Performance.SuccessRate > 0.9) score += 0.1;

        // Boost score for recent usage
        if (model.LastUsed > DateTime.UtcNow.AddDays(-7)) score += 0.1;

        return Math.Min(1.0, score);
    }

    private async Task<PerformanceTest> RunPerformanceTestAsync(string modelName, PerformanceTest test, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var tokensGenerated = 0;

        try
        {
            await foreach (var chunk in StreamCompletionAsync(modelName, test.Prompt, cancellationToken))
            {
                tokensGenerated += EstimateTokenCount(chunk);
                if (tokensGenerated >= test.ExpectedTokens) break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Performance test failed for model {ModelName}", modelName);
        }

        stopwatch.Stop();

        return new PerformanceTest
        {
            Name = test.Name,
            Prompt = test.Prompt,
            ExpectedTokens = test.ExpectedTokens,
            ActualTokens = tokensGenerated,
            ResponseTime = stopwatch.Elapsed,
            TokensPerSecond = tokensGenerated / Math.Max(stopwatch.Elapsed.TotalSeconds, 0.001),
            Success = tokensGenerated > 0
        };
    }

    private double CalculateTokensPerSecond(List<PerformanceTest> testResults)
    {
        if (!testResults.Any()) return 0.0;
        return testResults.Average(t => t.TokensPerSecond);
    }

    private int EstimateTokenCount(string text)
    {
        // Rough estimation: 1 token ≈ 4 characters for English text
        return Math.Max(1, text.Length / 4);
    }

    private async Task<double> GetModelMemoryUsageAsync(string modelName, CancellationToken cancellationToken)
    {
        // TODO: Implement actual memory usage monitoring
        return 0.0;
    }

    private async Task<double> GetModelCpuUsageAsync(string modelName, CancellationToken cancellationToken)
    {
        // TODO: Implement actual CPU usage monitoring
        return 0.0;
    }

    private async IAsyncEnumerable<string> StreamCompletionAsync(string model, string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var requestData = new
        {
            model,
            prompt,
            stream = true
        };

        var content = new StringContent(JsonSerializer.Serialize(requestData), System.Text.Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/generate") { Content = content };

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            yield return $"Error: Could not get a response from model '{model}'.";
            yield break;
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrEmpty(line)) continue;

            var json = JsonSerializer.Deserialize<JsonElement>(line);
            if (json.TryGetProperty("response", out var responseElement))
            {
                var chunk = responseElement.GetString();
                if (!string.IsNullOrEmpty(chunk))
                {
                    yield return chunk;
                }
            }
        }
    }
}

/// <summary>
/// Model performance profile
/// </summary>
public class ModelPerformanceProfile
{
    public string ModelName { get; set; } = string.Empty;
    public TimeSpan ColdStartTime { get; set; }
    public TimeSpan WarmResponseTime { get; set; }
    public double TokensPerSecond { get; set; }
    public double MemoryPeakMB { get; set; }
    public double CpuPeakPercent { get; set; }
    public QualityMetrics Quality { get; set; } = new();
    public List<PerformanceTest> TestResults { get; set; } = new();
}

/// <summary>
/// Performance test
/// </summary>
public class PerformanceTest
{
    public string Name { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public int ExpectedTokens { get; set; }
    public int ActualTokens { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public double TokensPerSecond { get; set; }
    public bool Success { get; set; }
}

/// <summary>
/// Quality metrics
/// </summary>
public class QualityMetrics
{
    public double CoherenceScore { get; set; }
    public double RelevanceScore { get; set; }
    public double CreativityScore { get; set; }
    public double FactualAccuracyScore { get; set; }
    public double ResponseLengthScore { get; set; }
}
