using Genisis.Core.Models;
using Genisis.Core.Repositories;
using Genisis.Core.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Genisis.Infrastructure.Caching;

/// <summary>
/// Cached wrapper for universe repository
/// </summary>
public class CachedUniverseRepository : IUniverseRepository
{
    private readonly IUniverseRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedUniverseRepository> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public CachedUniverseRepository(IUniverseRepository repository, ICacheService cacheService, ILogger<CachedUniverseRepository> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Universe> AddAsync(Universe universe, CancellationToken cancellationToken = default)
    {
        var result = await _repository.AddAsync(universe, cancellationToken);
        
        // Invalidate related caches
        await InvalidateUniverseCachesAsync();
        
        return result;
    }

    public async Task UpdateAsync(Universe entity, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAsync(entity, cancellationToken);
        
        // Invalidate specific universe cache
        await InvalidateUniverseCacheAsync(entity.Id);
    }

    public async Task DeleteAsync(Universe entity, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(entity, cancellationToken);
        
        // Invalidate specific universe cache
        await InvalidateUniverseCacheAsync(entity.Id);
    }

    public async Task<Universe?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"universe:{id}";
        
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _repository.GetByIdAsync(id, cancellationToken),
            _cacheExpiration,
            cancellationToken);
    }

    public async Task<List<Universe>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "universes:all";
        
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _repository.GetAllAsync(cancellationToken),
            _cacheExpiration,
            cancellationToken);
    }

    public async Task<Universe?> GetByIdWithContentAsync(UniverseId id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"universe:{id.Value}:with-content";
        
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _repository.GetByIdWithContentAsync(id, cancellationToken),
            _cacheExpiration,
            cancellationToken);
    }

    public async Task<Universe?> GetByNameAsync(EntityName name, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"universe:name:{name.Value}";
        
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _repository.GetByNameAsync(name, cancellationToken),
            _cacheExpiration,
            cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(EntityName name, UniverseId? excludeId = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = excludeId.HasValue 
            ? $"universe:name-unique:{name.Value}:exclude:{excludeId.Value.Value}"
            : $"universe:name-unique:{name.Value}";
        
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _repository.IsNameUniqueAsync(name, excludeId, cancellationToken),
            TimeSpan.FromMinutes(5), // Shorter cache for uniqueness checks
            cancellationToken);
    }

    public async Task<List<Universe>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"universes:search:{searchTerm.ToLowerInvariant()}";
        
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _repository.SearchAsync(searchTerm, cancellationToken),
            TimeSpan.FromMinutes(10), // Shorter cache for search results
            cancellationToken);
    }

    public async Task<List<Universe>> GetAllOrderedByNameAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "universes:ordered-by-name";
        
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _repository.GetAllOrderedByNameAsync(cancellationToken),
            _cacheExpiration,
            cancellationToken);
    }

    private async Task InvalidateUniverseCacheAsync(int universeId)
    {
        var keysToRemove = new[]
        {
            $"universe:{universeId}",
            $"universe:{universeId}:with-content",
            "universes:all",
            "universes:ordered-by-name"
        };

        await _cacheService.RemoveAsync(keysToRemove);
        _logger.LogDebug("Invalidated cache for universe {UniverseId}", universeId);
    }

    private async Task InvalidateUniverseCachesAsync()
    {
        var keysToRemove = new[]
        {
            "universes:all",
            "universes:ordered-by-name"
        };

        await _cacheService.RemoveAsync(keysToRemove);
        _logger.LogDebug("Invalidated universe list caches");
    }
}
