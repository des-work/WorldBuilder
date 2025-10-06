using Genisis.Core.ValueObjects;

namespace Genisis.Core.DomainEvents;

/// <summary>
/// Domain event raised when a story is created
/// </summary>
public class StoryCreatedEvent : DomainEvent
{
    public StoryId StoryId { get; }
    public UniverseId UniverseId { get; }
    public EntityName Name { get; }
    public StoryLogline? Logline { get; }

    public StoryCreatedEvent(StoryId storyId, UniverseId universeId, EntityName name, StoryLogline? logline)
    {
        StoryId = storyId;
        UniverseId = universeId;
        Name = name;
        Logline = logline;
    }
}

/// <summary>
/// Domain event raised when a story is updated
/// </summary>
public class StoryUpdatedEvent : DomainEvent
{
    public StoryId StoryId { get; }
    public UniverseId UniverseId { get; }
    public EntityName Name { get; }
    public StoryLogline? Logline { get; }

    public StoryUpdatedEvent(StoryId storyId, UniverseId universeId, EntityName name, StoryLogline? logline)
    {
        StoryId = storyId;
        UniverseId = universeId;
        Name = name;
        Logline = logline;
    }
}

/// <summary>
/// Domain event raised when a story is deleted
/// </summary>
public class StoryDeletedEvent : DomainEvent
{
    public StoryId StoryId { get; }
    public UniverseId UniverseId { get; }
    public EntityName Name { get; }

    public StoryDeletedEvent(StoryId storyId, UniverseId universeId, EntityName name)
    {
        StoryId = storyId;
        UniverseId = universeId;
        Name = name;
    }
}
