using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genisis.Core.Services;

/// <summary>
/// Interface for initial setup service
/// </summary>
public interface IInitialSetupService
{
    /// <summary>
    /// Get current setup status
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Setup status</returns>
    Task<SetupStatus> GetSetupStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Start initial setup process
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Setup progress</returns>
    Task<SetupProgress> StartSetupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Continue setup with specific step
    /// </summary>
    /// <param name="step">Setup step</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Setup progress</returns>
    Task<SetupProgress> ContinueSetupAsync(SetupStep step, CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete setup process
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task CompleteSetupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Skip setup process
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task SkipSetupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset setup process
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task ResetSetupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get setup step data
    /// </summary>
    /// <param name="step">Setup step</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Setup step data</returns>
    Task<SetupStepData> GetSetupStepDataAsync(SetupStep step, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate setup step
    /// </summary>
    /// <param name="step">Setup step</param>
    /// <param name="data">Step data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<SetupValidationResult> ValidateSetupStepAsync(SetupStep step, SetupStepData data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save setup step data
    /// </summary>
    /// <param name="step">Setup step</param>
    /// <param name="data">Step data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task SaveSetupStepDataAsync(SetupStep step, SetupStepData data, CancellationToken cancellationToken = default);
}
