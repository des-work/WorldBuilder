using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Genisis.Infrastructure.Caching;

/// <summary>
/// Memory-based cache service implementation
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting value from cache with key: {Key}", key);
            var value = _memoryCache.Get<T>(key);
            return Task.FromResult(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache with key: {Key}", key);
            return Task.FromResult<T?>(default);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Setting value in cache with key: {Key}", key);
            var options = new MemoryCacheEntryOptions();
            
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }
            else
            {
                // Default expiration of 1 hour
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            }

            _memoryCache.Set(key, value, options);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache with key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Removing value from cache with key: {Key}", key);
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from cache with key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemoveAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Removing {Count} values from cache", keys.Count());
            foreach (var key in keys)
            {
                _memoryCache.Remove(key);
            }
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing values from cache");
            return Task.CompletedTask;
        }
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Clearing all values from cache");
            if (_memoryCache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0);
            }
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return Task.CompletedTask;
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting or setting value in cache with key: {Key}", key);
            
            var value = await GetAsync<T>(key, cancellationToken);
            if (value != null)
            {
                return value;
            }

            var newValue = await factory();
            await SetAsync(key, newValue, expiration, cancellationToken);
            return newValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting or setting value in cache with key: {Key}", key);
            throw;
        }
    }
}
