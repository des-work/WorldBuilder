namespace Genisis.Core.DomainEvents;

/// <summary>
/// Marker interface for domain events
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this domain event
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// When this event occurred
    /// </summary>
    DateTime OccurredOn { get; }
}

/// <summary>
/// Base implementation for domain events
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }

    public Guid Id { get; }
    public DateTime OccurredOn { get; }
}
