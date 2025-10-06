using Genisis.Core.Models;
using Genisis.Core.ValueObjects;

namespace Genisis.Core.Repositories;

public interface IUniverseRepository : IRepository<Universe>
{
    /// <summary>
    /// Gets a universe by ID with its stories and characters
    /// </summary>
    Task<Universe?> GetByIdWithContentAsync(UniverseId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a universe by name
    /// </summary>
    Task<Universe?> GetByNameAsync(EntityName name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a universe name is unique
    /// </summary>
    Task<bool> IsNameUniqueAsync(EntityName name, UniverseId? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets universes matching a search term
    /// </summary>
    Task<List<Universe>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all universes ordered by name
    /// </summary>
    Task<List<Universe>> GetAllOrderedByNameAsync(CancellationToken cancellationToken = default);
}
