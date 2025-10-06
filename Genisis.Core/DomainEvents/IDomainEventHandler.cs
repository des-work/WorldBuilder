namespace Genisis.Core.DomainEvents;

/// <summary>
/// Interface for handling domain events
/// </summary>
/// <typeparam name="T">The type of domain event to handle</typeparam>
public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    /// <summary>
    /// Handles the domain event
    /// </summary>
    /// <param name="domainEvent">The domain event to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task HandleAsync(T domainEvent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for publishing domain events
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publishes a domain event
    /// </summary>
    /// <param name="domainEvent">The domain event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes multiple domain events
    /// </summary>
    /// <param name="domainEvents">The domain events to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
