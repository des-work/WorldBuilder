using Genisis.Core.ValueObjects;

namespace Genisis.Core.DomainEvents;

/// <summary>
/// Domain event raised when a character is created
/// </summary>
public class CharacterCreatedEvent : DomainEvent
{
    public CharacterId CharacterId { get; }
    public UniverseId UniverseId { get; }
    public EntityName Name { get; }
    public CharacterTier Tier { get; }
    public CharacterBio? Bio { get; }
    public CharacterNotes? Notes { get; }

    public CharacterCreatedEvent(CharacterId characterId, UniverseId universeId, EntityName name, 
        CharacterTier tier, CharacterBio? bio, CharacterNotes? notes)
    {
        CharacterId = characterId;
        UniverseId = universeId;
        Name = name;
        Tier = tier;
        Bio = bio;
        Notes = notes;
    }
}

/// <summary>
/// Domain event raised when a character is updated
/// </summary>
public class CharacterUpdatedEvent : DomainEvent
{
    public CharacterId CharacterId { get; }
    public UniverseId UniverseId { get; }
    public EntityName Name { get; }
    public CharacterTier Tier { get; }
    public CharacterBio? Bio { get; }
    public CharacterNotes? Notes { get; }

    public CharacterUpdatedEvent(CharacterId characterId, UniverseId universeId, EntityName name, 
        CharacterTier tier, CharacterBio? bio, CharacterNotes? notes)
    {
        CharacterId = characterId;
        UniverseId = universeId;
        Name = name;
        Tier = tier;
        Bio = bio;
        Notes = notes;
    }
}

/// <summary>
/// Domain event raised when a character is deleted
/// </summary>
public class CharacterDeletedEvent : DomainEvent
{
    public CharacterId CharacterId { get; }
    public UniverseId UniverseId { get; }
    public EntityName Name { get; }

    public CharacterDeletedEvent(CharacterId characterId, UniverseId universeId, EntityName name)
    {
        CharacterId = characterId;
        UniverseId = universeId;
        Name = name;
    }
}
