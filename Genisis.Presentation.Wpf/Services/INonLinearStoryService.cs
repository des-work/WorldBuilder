using Genisis.Presentation.Wpf.ViewModels;

namespace Genisis.Presentation.Wpf.Services;

/// <summary>
/// Service interface for non-linear story creation operations
/// </summary>
public interface INonLinearStoryService
{
    /// <summary>
    /// Create a new story element
    /// </summary>
    /// <param name="type">Type of element to create</param>
    /// <param name="title">Title of the element</param>
    /// <param name="description">Description of the element</param>
    /// <returns>Created story element</returns>
    Task<StoryElement> CreateElementAsync(ElementType type, string title, string description);

    /// <summary>
    /// Link two story elements together
    /// </summary>
    /// <param name="element1">First element</param>
    /// <param name="element2">Second element</param>
    /// <returns>True if linking was successful</returns>
    Task<bool> LinkElementsAsync(StoryElement element1, StoryElement element2);

    /// <summary>
    /// Search for story elements
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>Matching story elements</returns>
    Task<IEnumerable<StoryElement>> SearchElementsAsync(string searchTerm);

    /// <summary>
    /// Get story element by ID
    /// </summary>
    /// <param name="id">Element ID</param>
    /// <returns>Story element or null if not found</returns>
    Task<StoryElement?> GetElementByIdAsync(Guid id);

    /// <summary>
    /// Get all story elements
    /// </summary>
    /// <returns>All story elements</returns>
    Task<IEnumerable<StoryElement>> GetAllElementsAsync();

    /// <summary>
    /// Update a story element
    /// </summary>
    /// <param name="element">Element to update</param>
    /// <returns>Updated element</returns>
    Task<StoryElement> UpdateElementAsync(StoryElement element);

    /// <summary>
    /// Delete a story element
    /// </summary>
    /// <param name="id">Element ID to delete</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteElementAsync(Guid id);

    /// <summary>
    /// Get elements by type
    /// </summary>
    /// <param name="type">Element type</param>
    /// <returns>Elements of the specified type</returns>
    Task<IEnumerable<StoryElement>> GetElementsByTypeAsync(ElementType type);

    /// <summary>
    /// Get recent elements
    /// </summary>
    /// <param name="count">Number of recent elements to return</param>
    /// <returns>Recent elements</returns>
    Task<IEnumerable<StoryElement>> GetRecentElementsAsync(int count = 10);

    /// <summary>
    /// Get favorite elements
    /// </summary>
    /// <returns>Favorite elements</returns>
    Task<IEnumerable<StoryElement>> GetFavoriteElementsAsync();

    /// <summary>
    /// Add element to favorites
    /// </summary>
    /// <param name="id">Element ID</param>
    /// <returns>True if successful</returns>
    Task<bool> AddToFavoritesAsync(Guid id);

    /// <summary>
    /// Remove element from favorites
    /// </summary>
    /// <param name="id">Element ID</param>
    /// <returns>True if successful</returns>
    Task<bool> RemoveFromFavoritesAsync(Guid id);

    /// <summary>
    /// Get element relationships
    /// </summary>
    /// <param name="id">Element ID</param>
    /// <returns>Related elements</returns>
    Task<IEnumerable<StoryElement>> GetElementRelationshipsAsync(Guid id);

    /// <summary>
    /// Load large dataset for performance testing
    /// </summary>
    /// <param name="count">Number of elements to create</param>
    /// <returns>Task representing the operation</returns>
    Task LoadLargeDatasetAsync(int count);
}
