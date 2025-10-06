using Genisis.Core.Models;
using Genisis.Core.ValueObjects;

namespace Genisis.Core.Repositories;

public interface IStoryRepository : IRepository<Story>
{
    /// <summary>
    /// Gets stories by universe ID
    /// </summary>
    Task<List<Story>> GetByUniverseIdAsync(UniverseId universeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a story by name within a universe
    /// </summary>
    Task<Story?> GetByNameInUniverseAsync(EntityName name, UniverseId universeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a story name is unique within a universe
    /// </summary>
    Task<bool> IsNameUniqueInUniverseAsync(EntityName name, UniverseId universeId, StoryId? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a story with its chapters
    /// </summary>
    Task<Story?> GetByIdWithChaptersAsync(StoryId id, CancellationToken cancellationToken = default);
}
