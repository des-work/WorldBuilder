using Genisis.Core.Models;
using Genisis.Core.ValueObjects;

namespace Genisis.Core.Repositories;

public interface IChapterRepository : IRepository<Chapter>
{
    /// <summary>
    /// Gets chapters by story ID
    /// </summary>
    Task<List<Chapter>> GetByStoryIdAsync(StoryId storyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets chapters by character ID
    /// </summary>
    Task<List<Chapter>> GetByCharacterIdAsync(CharacterId characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a chapter by title within a story
    /// </summary>
    Task<Chapter?> GetByTitleInStoryAsync(EntityName title, StoryId storyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a chapter title is unique within a story
    /// </summary>
    Task<bool> IsTitleUniqueInStoryAsync(EntityName title, StoryId storyId, ChapterId? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a chapter with its characters
    /// </summary>
    Task<Chapter?> GetByIdWithCharactersAsync(ChapterId id, CancellationToken cancellationToken = default);
}
