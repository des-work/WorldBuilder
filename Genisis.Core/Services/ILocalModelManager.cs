using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genisis.Core.Services;

/// <summary>
/// Interface for managing local AI models
/// </summary>
public interface ILocalModelManager
{
    /// <summary>
    /// Discover available local models
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of discovered models</returns>
    Task<IEnumerable<LocalModel>> DiscoverModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get specific model by name
    /// </summary>
    /// <param name="modelName">Model name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Model or null if not found</returns>
    Task<LocalModel?> GetModelAsync(string modelName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate model is working correctly
    /// </summary>
    /// <param name="modelName">Model name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if model is valid</returns>
    Task<bool> ValidateModelAsync(string modelName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance profile for model
    /// </summary>
    /// <param name="modelName">Model name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance profile</returns>
    Task<ModelPerformanceProfile> GetPerformanceProfileAsync(string modelName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recommended models for specific task type
    /// </summary>
    /// <param name="taskType">Task type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recommended models</returns>
    Task<IEnumerable<LocalModel>> GetRecommendedModelsAsync(AITaskType taskType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get model configuration
    /// </summary>
    /// <param name="modelName">Model name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Model configuration</returns>
    Task<ModelConfiguration> GetModelConfigurationAsync(string modelName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save model configuration
    /// </summary>
    /// <param name="modelName">Model name</param>
    /// <param name="config">Configuration to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task SaveModelConfigurationAsync(string modelName, ModelConfiguration config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update model usage statistics
    /// </summary>
    /// <param name="modelName">Model name</param>
    /// <param name="usage">Usage statistics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task UpdateModelUsageAsync(string modelName, ModelUsage usage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get model usage statistics
    /// </summary>
    /// <param name="modelName">Model name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Usage statistics</returns>
    Task<ModelUsage> GetModelUsageAsync(string modelName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all model usage statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All usage statistics</returns>
    Task<IEnumerable<ModelUsage>> GetAllModelUsageAsync(CancellationToken cancellationToken = default);
}
