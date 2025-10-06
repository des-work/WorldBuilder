using Genisis.Core.Models;
using Genisis.Core.ValueObjects;

namespace Genisis.Core.Repositories;

public interface ICharacterRepository : IRepository<Character>
{
    /// <summary>
    /// Gets characters by universe ID
    /// </summary>
    Task<List<Character>> GetByUniverseIdAsync(UniverseId universeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a character by name within a universe
    /// </summary>
    Task<Character?> GetByNameInUniverseAsync(EntityName name, UniverseId universeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a character name is unique within a universe
    /// </summary>
    Task<bool> IsNameUniqueInUniverseAsync(EntityName name, UniverseId universeId, CharacterId? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a character with its chapters
    /// </summary>
    Task<Character?> GetByIdWithChaptersAsync(CharacterId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets characters by tier within a universe
    /// </summary>
    Task<List<Character>> GetByTierAsync(CharacterTier tier, UniverseId universeId, CancellationToken cancellationToken = default);
}
