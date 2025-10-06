using Genisis.Core.Models;
using Genisis.Core.ValueObjects;

namespace Genisis.Core.Services;

/// <summary>
/// Domain service for complex story operations
/// </summary>
public interface IStoryDomainService
{
    /// <summary>
    /// Validates that a story name is unique within a universe
    /// </summary>
    Task<bool> IsNameUniqueInUniverseAsync(EntityName name, UniverseId universeId, StoryId? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a story can be deleted (no dependent chapters)
    /// </summary>
    Task<bool> CanDeleteAsync(Story story, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next chapter order for a story
    /// </summary>
    Task<int> GetNextChapterOrderAsync(Story story, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorders chapters in a story
    /// </summary>
    Task ReorderChaptersAsync(Story story, IEnumerable<ChapterId> chapterIdsInOrder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates story creation rules
    /// </summary>
    Task<ValidationResult> ValidateCreationAsync(EntityName name, StoryLogline? logline, UniverseId universeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates story update rules
    /// </summary>
    Task<ValidationResult> ValidateUpdateAsync(Story story, EntityName name, StoryLogline? logline, CancellationToken cancellationToken = default);
}
