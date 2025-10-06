using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Genisis.Core.ValueObjects;
using Genisis.Core.DomainEvents;

namespace Genisis.Core.Models;

/// <summary>
/// Represents a fictional universe containing stories and characters
/// </summary>
public class Universe
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected Universe() { } // EF Core constructor

    public Universe(EntityName name, EntityDescription? description = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new UniverseCreatedEvent(new UniverseId(Id), Name, Description));
    }

    [Key]
    public int Id { get; private set; }

    [Required]
    [StringLength(200)]
    public EntityName Name { get; private set; } = null!;

    [StringLength(2000)]
    public EntityDescription? Description { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public ICollection<Story> Stories { get; private set; } = new List<Story>();
    public ICollection<Character> Characters { get; private set; } = new List<Character>();

    // Domain events
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Updates the universe name and description
    /// </summary>
    public void Update(EntityName name, EntityDescription? description = null)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new UniverseUpdatedEvent(new UniverseId(Id), Name, Description));
    }

    /// <summary>
    /// Marks the universe for deletion
    /// </summary>
    public void Delete()
    {
        AddDomainEvent(new UniverseDeletedEvent(new UniverseId(Id), Name));
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
    /// Adds a story to this universe
    /// </summary>
    public Story AddStory(EntityName name, StoryLogline? logline = null)
    {
        var story = new Story(name, logline, new UniverseId(Id));
        Stories.Add(story);
        return story;
    }

    /// <summary>
    /// Adds a character to this universe
    /// </summary>
    public Character AddCharacter(EntityName name, CharacterTier tier, CharacterBio? bio = null, CharacterNotes? notes = null)
    {
        var character = new Character(name, tier, bio, notes, new UniverseId(Id));
        Characters.Add(character);
        return character;
    }
}
