using Genisis.Core.Models;
using Genisis.Core.ValueObjects;

namespace Genisis.Core.Services;

/// <summary>
/// Domain service for complex chapter operations
/// </summary>
public interface IChapterDomainService
{
    /// <summary>
    /// Validates that a chapter title is unique within a story
    /// </summary>
    Task<bool> IsTitleUniqueInStoryAsync(EntityName title, StoryId storyId, ChapterId? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a chapter can be deleted
    /// </summary>
    Task<bool> CanDeleteAsync(Chapter chapter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next chapter order for a story
    /// </summary>
    Task<int> GetNextChapterOrderAsync(Story story, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorders chapters in a story
    /// </summary>
    Task ReorderChaptersAsync(Story story, IEnumerable<ChapterId> chapterIdsInOrder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates chapter creation rules
    /// </summary>
    Task<ValidationResult> ValidateCreationAsync(EntityName title, int chapterOrder, ChapterContent? content, StoryId storyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates chapter update rules
    /// </summary>
    Task<ValidationResult> ValidateUpdateAsync(Chapter chapter, EntityName title, int chapterOrder, ChapterContent? content, CancellationToken cancellationToken = default);
}
