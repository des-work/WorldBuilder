using Genisis.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Genisis.Infrastructure.Data;

/// <summary>
/// Implementation of Unit of Work pattern
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly GenesisDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(GenesisDbContext context, ILogger<UnitOfWork> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IUniverseRepository Universes { get; private set; } = null!;
    public IStoryRepository Stories { get; private set; } = null!;
    public ICharacterRepository Characters { get; private set; } = null!;
    public IChapterRepository Chapters { get; private set; } = null!;

    public ITransaction? CurrentTransaction => _transaction != null ? new Transaction(_transaction) : null;

    /// <summary>
    /// Initializes the repositories
    /// </summary>
    public void InitializeRepositories(
        IUniverseRepository universeRepository,
        IStoryRepository storyRepository,
        ICharacterRepository characterRepository,
        IChapterRepository chapterRepository)
    {
        Universes = universeRepository ?? throw new ArgumentNullException(nameof(universeRepository));
        Stories = storyRepository ?? throw new ArgumentNullException(nameof(storyRepository));
        Characters = characterRepository ?? throw new ArgumentNullException(nameof(characterRepository));
        Chapters = chapterRepository ?? throw new ArgumentNullException(nameof(chapterRepository));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Saving changes to database");
            var result = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Successfully saved {Count} changes to database", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to database");
            throw;
        }
    }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress");
        }

        try
        {
            _logger.LogDebug("Beginning database transaction");
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return new Transaction(_transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error beginning database transaction");
            throw;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
        await _context.DisposeAsync();
    }
}

/// <summary>
/// Implementation of database transaction
/// </summary>
public class Transaction : ITransaction
{
    private readonly IDbContextTransaction _transaction;
    private readonly ILogger<Transaction>? _logger;

    public Transaction(IDbContextTransaction transaction, ILogger<Transaction>? logger = null)
    {
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        _logger = logger;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogDebug("Committing database transaction");
            await _transaction.CommitAsync(cancellationToken);
            _logger?.LogDebug("Successfully committed database transaction");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error committing database transaction");
            throw;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogDebug("Rolling back database transaction");
            await _transaction.RollbackAsync(cancellationToken);
            _logger?.LogDebug("Successfully rolled back database transaction");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error rolling back database transaction");
            throw;
        }
    }

    public void Dispose()
    {
        _transaction.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _transaction.DisposeAsync();
    }
}
