using Genisis.Core.Models;
using Genisis.Core.ValueObjects;

namespace Genisis.Infrastructure.Specifications;

/// <summary>
/// Specification for finding characters by universe ID
/// </summary>
public class CharactersByUniverseIdSpecification : Specification<Character>
{
    public CharactersByUniverseIdSpecification(UniverseId universeId)
    {
        Criteria = c => c.UniverseId.Value == universeId.Value;
        AddOrderBy(c => c.Name.Value);
        IsTrackingEnabled = false;
    }
}

/// <summary>
/// Specification for finding characters by name within a universe
/// </summary>
public class CharacterByNameInUniverseSpecification : Specification<Character>
{
    public CharacterByNameInUniverseSpecification(EntityName name, UniverseId universeId)
    {
        Criteria = c => c.Name.Value == name.Value && c.UniverseId.Value == universeId.Value;
        IsTrackingEnabled = false;
    }
}

/// <summary>
/// Specification for finding characters by name within a universe excluding a specific ID
/// </summary>
public class CharacterByNameInUniverseExcludingIdSpecification : Specification<Character>
{
    public CharacterByNameInUniverseExcludingIdSpecification(EntityName name, UniverseId universeId, CharacterId excludeId)
    {
        Criteria = c => c.Name.Value == name.Value && 
                       c.UniverseId.Value == universeId.Value && 
                       c.Id != excludeId.Value;
        IsTrackingEnabled = false;
    }
}

/// <summary>
/// Specification for getting character with chapters
/// </summary>
public class CharacterWithChaptersSpecification : Specification<Character>
{
    public CharacterWithChaptersSpecification(CharacterId characterId)
    {
        Criteria = c => c.Id == characterId.Value;
        AddInclude(c => c.Chapters);
        IsTrackingEnabled = false;
    }
}

/// <summary>
/// Specification for finding characters by tier
/// </summary>
public class CharactersByTierSpecification : Specification<Character>
{
    public CharactersByTierSpecification(CharacterTier tier, UniverseId universeId)
    {
        Criteria = c => c.Tier == tier && c.UniverseId.Value == universeId.Value;
        AddOrderBy(c => c.Name.Value);
        IsTrackingEnabled = false;
    }
}
