using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Genisis.Core.ValueObjects;
using Genisis.Core.DomainEvents;

namespace Genisis.Core.Models;

/// <summary>
/// Represents a chapter within a story
/// </summary>
public class Chapter
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected Chapter() { } // EF Core constructor

    public Chapter(EntityName title, int chapterOrder, ChapterContent? content, StoryId storyId)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        ChapterOrder = chapterOrder;
        Content = content;
        StoryId = storyId;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ChapterCreatedEvent(new ChapterId(Id), StoryId, Title, ChapterOrder, Content));
    }

    [Key]
    public int Id { get; private set; }

    [Required]
    [StringLength(200)]
    public EntityName Title { get; private set; } = null!;

    [Range(0, int.MaxValue)]
    public int ChapterOrder { get; private set; }

    [StringLength(10000)]
    public ChapterContent? Content { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    [Required]
    public StoryId StoryId { get; private set; }
    
    [ForeignKey(nameof(StoryId))]
    public Story? Story { get; private set; }
    
    public ICollection<Character> Characters { get; private set; } = new List<Character>();

    // Domain events
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Updates the chapter information
    /// </summary>
    public void Update(EntityName title, int chapterOrder, ChapterContent? content = null)
    {
        if (title == null) throw new ArgumentNullException(nameof(title));
        
        Title = title;
        ChapterOrder = chapterOrder;
        Content = content;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ChapterUpdatedEvent(new ChapterId(Id), StoryId, Title, ChapterOrder, Content));
    }

    /// <summary>
    /// Marks the chapter for deletion
    /// </summary>
    public void Delete()
    {
        AddDomainEvent(new ChapterDeletedEvent(new ChapterId(Id), StoryId, Title));
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
    /// Adds a character to this chapter
    /// </summary>
    public void AddCharacter(Character character)
    {
        if (character == null) throw new ArgumentNullException(nameof(character));
        
        if (!Characters.Contains(character))
        {
            Characters.Add(character);
            character.AddToChapter(this);
        }
    }

    /// <summary>
    /// Removes a character from this chapter
    /// </summary>
    public void RemoveCharacter(Character character)
    {
        if (character == null) throw new ArgumentNullException(nameof(character));
        
        Characters.Remove(character);
        character.RemoveFromChapter(this);
    }
}
