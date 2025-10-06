using System.Collections.Concurrent;
using System.Diagnostics;
using Genisis.Presentation.Wpf.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Resilient implementation of non-linear story service with error handling and performance optimization
/// </summary>
public class ResilientNonLinearStoryService : INonLinearStoryService
{
    private readonly INonLinearStoryService _baseService;
    private readonly ILogger<ResilientNonLinearStoryService> _logger;
    private readonly IRetryPolicy _retryPolicy;
    private readonly ICircuitBreaker _circuitBreaker;
    private readonly IMemoryCache _cache;
    private readonly IBackgroundProcessingService _backgroundService;
    private readonly ConcurrentDictionary<Guid, StoryElement> _localCache = new();

    public ResilientNonLinearStoryService(
        INonLinearStoryService baseService,
        ILogger<ResilientNonLinearStoryService> logger,
        IRetryPolicy retryPolicy,
        ICircuitBreaker circuitBreaker,
        IMemoryCache cache,
        IBackgroundProcessingService backgroundService)
    {
        _baseService = baseService ?? throw new ArgumentNullException(nameof(baseService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        _circuitBreaker = circuitBreaker ?? throw new ArgumentNullException(nameof(circuitBreaker));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _backgroundService = backgroundService ?? throw new ArgumentNullException(nameof(backgroundService));
    }

    public async Task<StoryElement> CreateElementAsync(ElementType type, string title, string description)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Creating element {Type} with title {Title}", type, title);

            var element = await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _baseService.CreateElementAsync(type, title, description);
                });
            });

            // Cache the created element
            _localCache.TryAdd(element.Id, element);
            _cache.Set($"element_{element.Id}", element, TimeSpan.FromMinutes(30));

            stopwatch.Stop();
            _logger.LogDebug("Created element {ElementId} in {ElapsedMs}ms", element.Id, stopwatch.ElapsedMilliseconds);

            return element;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to create element {Type} with title {Title} after {ElapsedMs}ms", 
                type, title, stopwatch.ElapsedMilliseconds);
            
            // Graceful degradation - return a placeholder element
            var fallbackElement = new StoryElement(type, $"{title} (Offline)", $"{description} - Created offline due to error");
            _localCache.TryAdd(fallbackElement.Id, fallbackElement);
            
            // Queue for background retry
            await _backgroundService.EnqueueTaskAsync(
                async (service) => await service.CreateElementAsync(type, title, description),
                _baseService,
                TaskPriority.High);

            return fallbackElement;
        }
    }

    public async Task<bool> LinkElementsAsync(StoryElement element1, StoryElement element2)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Linking elements {Element1} and {Element2}", element1.Title, element2.Title);

            var result = await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _baseService.LinkElementsAsync(element1, element2);
                });
            });

            stopwatch.Stop();
            _logger.LogDebug("Linked elements {Element1} and {Element2} in {ElapsedMs}ms", 
                element1.Title, element2.Title, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to link elements {Element1} and {Element2} after {ElapsedMs}ms", 
                element1.Title, element2.Title, stopwatch.ElapsedMilliseconds);
            
            // Graceful degradation - return false but don't crash
            return false;
        }
    }

    public async Task<IEnumerable<StoryElement>> SearchElementsAsync(string searchTerm)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Searching elements with term {SearchTerm}", searchTerm);

            // Check cache first
            var cacheKey = $"search_{searchTerm}";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<StoryElement>? cachedResults))
            {
                stopwatch.Stop();
                _logger.LogDebug("Search cache hit for {SearchTerm} in {ElapsedMs}ms", 
                    searchTerm, stopwatch.ElapsedMilliseconds);
                return cachedResults!;
            }

            var results = await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _baseService.SearchElementsAsync(searchTerm);
                });
            });

            // Cache results
            _cache.Set(cacheKey, results, TimeSpan.FromMinutes(10));

            stopwatch.Stop();
            _logger.LogDebug("Search completed for {SearchTerm} in {ElapsedMs}ms, found {Count} results", 
                searchTerm, stopwatch.ElapsedMilliseconds, results.Count());

            return results;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Search failed for {SearchTerm} after {ElapsedMs}ms", 
                searchTerm, stopwatch.ElapsedMilliseconds);
            
            // Graceful degradation - return empty results
            return Enumerable.Empty<StoryElement>();
        }
    }

    public async Task<StoryElement?> GetElementByIdAsync(Guid id)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Getting element by ID {ElementId}", id);

            // Check local cache first
            if (_localCache.TryGetValue(id, out var localElement))
            {
                stopwatch.Stop();
                _logger.LogDebug("Local cache hit for element {ElementId} in {ElapsedMs}ms", 
                    id, stopwatch.ElapsedMilliseconds);
                return localElement;
            }

            // Check memory cache
            var cacheKey = $"element_{id}";
            if (_cache.TryGetValue(cacheKey, out StoryElement? cachedElement))
            {
                _localCache.TryAdd(id, cachedElement!);
                stopwatch.Stop();
                _logger.LogDebug("Memory cache hit for element {ElementId} in {ElapsedMs}ms", 
                    id, stopwatch.ElapsedMilliseconds);
                return cachedElement;
            }

            var element = await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _baseService.GetElementByIdAsync(id);
                });
            });

            if (element != null)
            {
                _localCache.TryAdd(id, element);
                _cache.Set(cacheKey, element, TimeSpan.FromMinutes(30));
            }

            stopwatch.Stop();
            _logger.LogDebug("Retrieved element {ElementId} in {ElapsedMs}ms", id, stopwatch.ElapsedMilliseconds);

            return element;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to get element {ElementId} after {ElapsedMs}ms", 
                id, stopwatch.ElapsedMilliseconds);
            
            // Graceful degradation - return null
            return null;
        }
    }

    public async Task<IEnumerable<StoryElement>> GetAllElementsAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Getting all elements");

            var elements = await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _baseService.GetAllElementsAsync();
                });
            });

            // Update local cache
            foreach (var element in elements)
            {
                _localCache.AddOrUpdate(element.Id, element, (key, oldValue) => element);
            }

            stopwatch.Stop();
            _logger.LogDebug("Retrieved {Count} elements in {ElapsedMs}ms", 
                elements.Count(), stopwatch.ElapsedMilliseconds);

            return elements;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to get all elements after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            
            // Graceful degradation - return cached elements
            return _localCache.Values;
        }
    }

    public async Task<StoryElement> UpdateElementAsync(StoryElement element)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Updating element {ElementId}", element.Id);

            var updatedElement = await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _baseService.UpdateElementAsync(element);
                });
            });

            // Update caches
            _localCache.AddOrUpdate(element.Id, updatedElement, (key, oldValue) => updatedElement);
            _cache.Set($"element_{element.Id}", updatedElement, TimeSpan.FromMinutes(30));

            stopwatch.Stop();
            _logger.LogDebug("Updated element {ElementId} in {ElapsedMs}ms", 
                element.Id, stopwatch.ElapsedMilliseconds);

            return updatedElement;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to update element {ElementId} after {ElapsedMs}ms", 
                element.Id, stopwatch.ElapsedMilliseconds);
            
            // Graceful degradation - return original element
            return element;
        }
    }

    public async Task<bool> DeleteElementAsync(Guid id)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Deleting element {ElementId}", id);

            var result = await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _baseService.DeleteElementAsync(id);
                });
            });

            if (result)
            {
                // Remove from caches
                _localCache.TryRemove(id, out _);
                _cache.Remove($"element_{id}");
            }

            stopwatch.Stop();
            _logger.LogDebug("Deleted element {ElementId} in {ElapsedMs}ms", id, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to delete element {ElementId} after {ElapsedMs}ms", 
                id, stopwatch.ElapsedMilliseconds);
            
            // Graceful degradation - return false
            return false;
        }
    }

    public async Task<IEnumerable<StoryElement>> GetElementsByTypeAsync(ElementType type)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Getting elements by type {Type}", type);

            var elements = await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _baseService.GetElementsByTypeAsync(type);
                });
            });

            stopwatch.Stop();
            _logger.LogDebug("Retrieved {Count} elements of type {Type} in {ElapsedMs}ms", 
                elements.Count(), type, stopwatch.ElapsedMilliseconds);

            return elements;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to get elements by type {Type} after {ElapsedMs}ms", 
                type, stopwatch.ElapsedMilliseconds);
            
            // Graceful degradation - return cached elements of this type
            return _localCache.Values.Where(e => e.ElementType == type);
        }
    }

    public async Task<IEnumerable<StoryElement>> GetRecentElementsAsync(int count = 10)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Getting {Count} recent elements", count);

            var elements = await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _baseService.GetRecentElementsAsync(count);
                });
            });

            stopwatch.Stop();
            _logger.LogDebug("Retrieved {Count} recent elements in {ElapsedMs}ms", 
                elements.Count(), stopwatch.ElapsedMilliseconds);

            return elements;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to get recent elements after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            
            // Graceful degradation - return empty list
            return Enumerable.Empty<StoryElement>();
        }
    }

    public async Task<IEnumerable<StoryElement>> GetFavoriteElementsAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Getting favorite elements");

            var elements = await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _baseService.GetFavoriteElementsAsync();
                });
            });

            stopwatch.Stop();
            _logger.LogDebug("Retrieved {Count} favorite elements in {ElapsedMs}ms", 
                elements.Count(), stopwatch.ElapsedMilliseconds);

            return elements;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to get favorite elements after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            
            // Graceful degradation - return empty list
            return Enumerable.Empty<StoryElement>();
        }
    }

    public async Task<bool> AddToFavoritesAsync(Guid id)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Adding element {ElementId} to favorites", id);

            var result = await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _baseService.AddToFavoritesAsync(id);
                });
            });

            stopwatch.Stop();
            _logger.LogDebug("Added element {ElementId} to favorites in {ElapsedMs}ms", 
                id, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to add element {ElementId} to favorites after {ElapsedMs}ms", 
                id, stopwatch.ElapsedMilliseconds);
            
            // Graceful degradation - return false
            return false;
        }
    }

    public async Task<bool> RemoveFromFavoritesAsync(Guid id)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Removing element {ElementId} from favorites", id);

            var result = await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _baseService.RemoveFromFavoritesAsync(id);
                });
            });

            stopwatch.Stop();
            _logger.LogDebug("Removed element {ElementId} from favorites in {ElapsedMs}ms", 
                id, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to remove element {ElementId} from favorites after {ElapsedMs}ms", 
                id, stopwatch.ElapsedMilliseconds);
            
            // Graceful degradation - return false
            return false;
        }
    }

    public async Task<IEnumerable<StoryElement>> GetElementRelationshipsAsync(Guid id)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Getting relationships for element {ElementId}", id);

            var relationships = await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _baseService.GetElementRelationshipsAsync(id);
                });
            });

            stopwatch.Stop();
            _logger.LogDebug("Retrieved {Count} relationships for element {ElementId} in {ElapsedMs}ms", 
                relationships.Count(), id, stopwatch.ElapsedMilliseconds);

            return relationships;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to get relationships for element {ElementId} after {ElapsedMs}ms", 
                id, stopwatch.ElapsedMilliseconds);
            
            // Graceful degradation - return empty list
            return Enumerable.Empty<StoryElement>();
        }
    }

    public async Task LoadLargeDatasetAsync(int count)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Loading large dataset with {Count} elements", count);

            await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    await _baseService.LoadLargeDatasetAsync(count);
                    return true;
                });
            });

            stopwatch.Stop();
            _logger.LogDebug("Loaded large dataset with {Count} elements in {ElapsedMs}ms", 
                count, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to load large dataset with {Count} elements after {ElapsedMs}ms", 
                count, stopwatch.ElapsedMilliseconds);
            
            // Graceful degradation - create placeholder elements
            for (int i = 0; i < Math.Min(count, 100); i++) // Limit to 100 for performance
            {
                var element = new StoryElement(ElementType.Character, $"Placeholder {i}", "Created offline");
                _localCache.TryAdd(element.Id, element);
            }
        }
    }
}
