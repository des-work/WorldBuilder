using Genisis.Core.Models;
using Genisis.Core.Repositories;
using Genisis.Infrastructure.Data;
using Genisis.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Genisis.Infrastructure.Repositories;

/// <summary>
/// Base repository implementation with common functionality
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly GenesisDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger _logger;

    protected BaseRepository(GenesisDbContext context, ILogger logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Adding {EntityType} to database", typeof(T).Name);
            await _dbSet.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Successfully added {EntityType} to database", typeof(T).Name);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding {EntityType} to database", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating {EntityType} in database", typeof(T).Name);
            _dbSet.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Successfully updated {EntityType} in database", typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {EntityType} in database", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting {EntityType} from database", typeof(T).Name);
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Successfully deleted {EntityType} from database", typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {EntityType} from database", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {EntityType} by ID {Id}", typeof(T).Name, id);
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} by ID {Id}", typeof(T).Name, id);
            throw;
        }
    }

    public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting all {EntityType} entities", typeof(T).Name);
            return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all {EntityType} entities", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Applies a specification to a query
    /// </summary>
    protected IQueryable<T> ApplySpecification(ISpecification<T> specification)
    {
        var query = _dbSet.AsQueryable();

        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy.Any())
        {
            var orderedQuery = query.OrderBy(specification.OrderBy.First());
            foreach (var orderBy in specification.OrderBy.Skip(1))
            {
                orderedQuery = orderedQuery.ThenBy(orderBy);
            }
            query = orderedQuery;
        }

        if (specification.OrderByDescending.Any())
        {
            var orderedQuery = query.OrderByDescending(specification.OrderByDescending.First());
            foreach (var orderByDescending in specification.OrderByDescending.Skip(1))
            {
                orderedQuery = orderedQuery.ThenByDescending(orderByDescending);
            }
            query = orderedQuery;
        }

        // Apply paging
        if (specification.Skip.HasValue)
        {
            query = query.Skip(specification.Skip.Value);
        }

        if (specification.Take.HasValue)
        {
            query = query.Take(specification.Take.Value);
        }

        // Apply tracking
        if (!specification.IsTrackingEnabled)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    /// <summary>
    /// Gets entities matching a specification
    /// </summary>
    protected async Task<List<T>> GetBySpecificationAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {EntityType} entities by specification", typeof(T).Name);
            var query = ApplySpecification(specification);
            return await query.ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} entities by specification", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Gets the first entity matching a specification
    /// </summary>
    protected async Task<T?> GetFirstBySpecificationAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting first {EntityType} entity by specification", typeof(T).Name);
            var query = ApplySpecification(specification);
            return await query.FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting first {EntityType} entity by specification", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Checks if any entities match a specification
    /// </summary>
    protected async Task<bool> AnyBySpecificationAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking if any {EntityType} entities match specification", typeof(T).Name);
            var query = ApplySpecification(specification);
            return await query.AnyAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if any {EntityType} entities match specification", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Gets the count of entities matching a specification
    /// </summary>
    protected async Task<int> CountBySpecificationAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Counting {EntityType} entities by specification", typeof(T).Name);
            var query = ApplySpecification(specification);
            return await query.CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting {EntityType} entities by specification", typeof(T).Name);
            throw;
        }
    }
}
