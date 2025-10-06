using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Genisis.Core.ValueObjects;
using Genisis.Core.DomainEvents;

namespace Genisis.Core.Models;

/// <summary>
/// Represents the importance level of a character in the story
/// </summary>
public enum CharacterTier
{
    Main,
    Recurring,
    Side
}

/// <summary>
/// Represents a character within a universe
/// </summary>
public class Character
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected Character() { } // EF Core constructor

    public Character(EntityName name, CharacterTier tier, CharacterBio? bio, CharacterNotes? notes, UniverseId universeId)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Tier = tier;
        Bio = bio;
        Notes = notes;
        UniverseId = universeId;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CharacterCreatedEvent(new CharacterId(Id), UniverseId, Name, Tier, Bio, Notes));
    }

    [Key]
    public int Id { get; private set; }

    [Required]
    [StringLength(200)]
    public EntityName Name { get; private set; } = null!;

    [Required]
    public CharacterTier Tier { get; private set; }

    [StringLength(5000)]
    public CharacterBio? Bio { get; private set; }

    [StringLength(2000)]
    public CharacterNotes? Notes { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    [Required]
    public UniverseId UniverseId { get; private set; }
    
    [ForeignKey(nameof(UniverseId))]
    public Universe? Universe { get; private set; }
    
    public ICollection<Chapter> Chapters { get; private set; } = new List<Chapter>();

    // Domain events
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Updates the character information
    /// </summary>
    public void Update(EntityName name, CharacterTier tier, CharacterBio? bio = null, CharacterNotes? notes = null)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        
        Name = name;
        Tier = tier;
        Bio = bio;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CharacterUpdatedEvent(new CharacterId(Id), UniverseId, Name, Tier, Bio, Notes));
    }

    /// <summary>
    /// Marks the character for deletion
    /// </summary>
    public void Delete()
    {
        AddDomainEvent(new CharacterDeletedEvent(new CharacterId(Id), UniverseId, Name));
    }

    /// <summary>
    /// Adds a domain event to the entity
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Adds this character to a chapter
    /// </summary>
    public void AddToChapter(Chapter chapter)
    {
        if (chapter == null) throw new ArgumentNullException(nameof(chapter));
        
        if (!Chapters.Contains(chapter))
        {
            Chapters.Add(chapter);
        }
    }

    /// <summary>
    /// Removes this character from a chapter
    /// </summary>
    public void RemoveFromChapter(Chapter chapter)
    {
        if (chapter == null) throw new ArgumentNullException(nameof(chapter));
        
        Chapters.Remove(chapter);
    }
}
