using Genisis.Core.ValueObjects;

namespace Genisis.Core.DomainEvents;

/// <summary>
/// Domain event raised when a chapter is created
/// </summary>
public class ChapterCreatedEvent : DomainEvent
{
    public ChapterId ChapterId { get; }
    public StoryId StoryId { get; }
    public EntityName Title { get; }
    public int ChapterOrder { get; }
    public ChapterContent? Content { get; }

    public ChapterCreatedEvent(ChapterId chapterId, StoryId storyId, EntityName title, 
        int chapterOrder, ChapterContent? content)
    {
        ChapterId = chapterId;
        StoryId = storyId;
        Title = title;
        ChapterOrder = chapterOrder;
        Content = content;
    }
}

/// <summary>
/// Domain event raised when a chapter is updated
/// </summary>
public class ChapterUpdatedEvent : DomainEvent
{
    public ChapterId ChapterId { get; }
    public StoryId StoryId { get; }
    public EntityName Title { get; }
    public int ChapterOrder { get; }
    public ChapterContent? Content { get; }

    public ChapterUpdatedEvent(ChapterId chapterId, StoryId storyId, EntityName title, 
        int chapterOrder, ChapterContent? content)
    {
        ChapterId = chapterId;
        StoryId = storyId;
        Title = title;
        ChapterOrder = chapterOrder;
        Content = content;
    }
}

/// <summary>
/// Domain event raised when a chapter is deleted
/// </summary>
public class ChapterDeletedEvent : DomainEvent
{
    public ChapterId ChapterId { get; }
    public StoryId StoryId { get; }
    public EntityName Title { get; }

    public ChapterDeletedEvent(ChapterId chapterId, StoryId storyId, EntityName title)
    {
        ChapterId = chapterId;
        StoryId = storyId;
        Title = title;
    }
}
