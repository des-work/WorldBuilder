using Genisis.Core.Models;
using Genisis.Core.ValueObjects;

namespace Genisis.Core.Services;

/// <summary>
/// Domain service for complex character operations
/// </summary>
public interface ICharacterDomainService
{
    /// <summary>
    /// Validates that a character name is unique within a universe
    /// </summary>
    Task<bool> IsNameUniqueInUniverseAsync(EntityName name, UniverseId universeId, CharacterId? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a character can be deleted (not referenced in chapters)
    /// </summary>
    Task<bool> CanDeleteAsync(Character character, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all chapters where a character appears
    /// </summary>
    Task<IEnumerable<Chapter>> GetCharacterChaptersAsync(Character character, CancellationToken cancellationToken = default);

    /// <summary>
    /// Links a character to a chapter
    /// </summary>
    Task LinkCharacterToChapterAsync(Character character, Chapter chapter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlinks a character from a chapter
    /// </summary>
    Task UnlinkCharacterFromChapterAsync(Character character, Chapter chapter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates character creation rules
    /// </summary>
    Task<ValidationResult> ValidateCreationAsync(EntityName name, CharacterTier tier, CharacterBio? bio, CharacterNotes? notes, UniverseId universeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates character update rules
    /// </summary>
    Task<ValidationResult> ValidateUpdateAsync(Character character, EntityName name, CharacterTier tier, CharacterBio? bio, CharacterNotes? notes, CancellationToken cancellationToken = default);
}
