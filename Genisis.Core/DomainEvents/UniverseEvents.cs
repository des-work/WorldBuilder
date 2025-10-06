using Genisis.Core.ValueObjects;

namespace Genisis.Core.DomainEvents;

/// <summary>
/// Domain event raised when a universe is created
/// </summary>
public class UniverseCreatedEvent : DomainEvent
{
    public UniverseId UniverseId { get; }
    public EntityName Name { get; }
    public EntityDescription? Description { get; }

    public UniverseCreatedEvent(UniverseId universeId, EntityName name, EntityDescription? description)
    {
        UniverseId = universeId;
        Name = name;
        Description = description;
    }
}

/// <summary>
/// Domain event raised when a universe is updated
/// </summary>
public class UniverseUpdatedEvent : DomainEvent
{
    public UniverseId UniverseId { get; }
    public EntityName Name { get; }
    public EntityDescription? Description { get; }

    public UniverseUpdatedEvent(UniverseId universeId, EntityName name, EntityDescription? description)
    {
        UniverseId = universeId;
        Name = name;
        Description = description;
    }
}

/// <summary>
/// Domain event raised when a universe is deleted
/// </summary>
public class UniverseDeletedEvent : DomainEvent
{
    public UniverseId UniverseId { get; }
    public EntityName Name { get; }

    public UniverseDeletedEvent(UniverseId universeId, EntityName name)
    {
        UniverseId = universeId;
        Name = name;
    }
}
