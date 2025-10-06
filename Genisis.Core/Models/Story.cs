using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Genisis.Core.ValueObjects;
using Genisis.Core.DomainEvents;

namespace Genisis.Core.Models;

/// <summary>
/// Represents a story within a universe containing chapters
/// </summary>
public class Story
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected Story() { } // EF Core constructor

    public Story(EntityName name, StoryLogline? logline, UniverseId universeId)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Logline = logline;
        UniverseId = universeId;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new StoryCreatedEvent(new StoryId(Id), UniverseId, Name, Logline));
    }

    [Key]
    public int Id { get; private set; }

    [Required]
    [StringLength(200)]
    public EntityName Name { get; private set; } = null!;

    [StringLength(500)]
    public StoryLogline? Logline { get; private set; }

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
    /// Updates the story name and logline
    /// </summary>
    public void Update(EntityName name, StoryLogline? logline = null)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        
        Name = name;
        Logline = logline;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new StoryUpdatedEvent(new StoryId(Id), UniverseId, Name, Logline));
    }

    /// <summary>
    /// Marks the story for deletion
    /// </summary>
    public void Delete()
    {
        AddDomainEvent(new StoryDeletedEvent(new StoryId(Id), UniverseId, Name));
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
    /// Adds a chapter to this story
    /// </summary>
    public Chapter AddChapter(EntityName title, ChapterContent? content = null)
    {
        var chapterOrder = Chapters.Count + 1;
        var chapter = new Chapter(title, chapterOrder, content, new StoryId(Id));
        Chapters.Add(chapter);
        return chapter;
    }
}
