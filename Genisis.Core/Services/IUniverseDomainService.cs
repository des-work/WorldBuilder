using Genisis.Core.Models;
using Genisis.Core.ValueObjects;

namespace Genisis.Core.Services;

/// <summary>
/// Domain service for complex universe operations
/// </summary>
public interface IUniverseDomainService
{
    /// <summary>
    /// Validates that a universe name is unique within the system
    /// </summary>
    Task<bool> IsNameUniqueAsync(EntityName name, UniverseId? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a universe can be deleted (no dependent stories or characters)
    /// </summary>
    Task<bool> CanDeleteAsync(Universe universe, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of stories and characters in a universe
    /// </summary>
    Task<(int StoryCount, int CharacterCount)> GetContentCountAsync(Universe universe, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates universe creation rules
    /// </summary>
    Task<ValidationResult> ValidateCreationAsync(EntityName name, EntityDescription? description, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates universe update rules
    /// </summary>
    Task<ValidationResult> ValidateUpdateAsync(Universe universe, EntityName name, EntityDescription? description, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of domain validation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; }
    public List<string> Errors { get; }

    private ValidationResult(bool isValid, List<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public static ValidationResult Success() => new(true, new List<string>());
    
    public static ValidationResult Failure(params string[] errors) => new(false, errors.ToList());
    
    public static ValidationResult Failure(IEnumerable<string> errors) => new(false, errors.ToList());
}
