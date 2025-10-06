using Genisis.Core.Repositories;

namespace Genisis.Infrastructure.Data;

/// <summary>
/// Unit of Work pattern for managing transactions and repositories
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the universe repository
    /// </summary>
    IUniverseRepository Universes { get; }

    /// <summary>
    /// Gets the story repository
    /// </summary>
    IStoryRepository Stories { get; }

    /// <summary>
    /// Gets the character repository
    /// </summary>
    ICharacterRepository Characters { get; }

    /// <summary>
    /// Gets the chapter repository
    /// </summary>
    IChapterRepository Chapters { get; }

    /// <summary>
    /// Saves all changes to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new transaction
    /// </summary>
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current transaction if one exists
    /// </summary>
    ITransaction? CurrentTransaction { get; }
}

/// <summary>
/// Represents a database transaction
/// </summary>
public interface ITransaction : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Commits the transaction
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the transaction
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
