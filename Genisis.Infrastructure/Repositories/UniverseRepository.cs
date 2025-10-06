using Genisis.Core.Models;
using Genisis.Core.Repositories;
using Genisis.Core.ValueObjects;
using Genisis.Infrastructure.Data;
using Genisis.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Genisis.Infrastructure.Repositories;

public class UniverseRepository : BaseRepository<Universe>, IUniverseRepository
{
    public UniverseRepository(GenesisDbContext context, ILogger<UniverseRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<Universe?> GetByIdWithContentAsync(UniverseId id, CancellationToken cancellationToken = default)
    {
        var specification = new UniverseWithContentSpecification(id);
        return await GetFirstBySpecificationAsync(specification, cancellationToken);
    }

    public async Task<Universe?> GetByNameAsync(EntityName name, CancellationToken cancellationToken = default)
    {
        var specification = new UniverseByNameSpecification(name);
        return await GetFirstBySpecificationAsync(specification, cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(EntityName name, UniverseId? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (excludeId.HasValue)
        {
            var specification = new UniverseByNameExcludingIdSpecification(name, excludeId.Value);
            return !await AnyBySpecificationAsync(specification, cancellationToken);
        }
        else
        {
            var specification = new UniverseByNameSpecification(name);
            return !await AnyBySpecificationAsync(specification, cancellationToken);
        }
    }

    public async Task<List<Universe>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var specification = new UniverseSearchSpecification(searchTerm);
        return await GetBySpecificationAsync(specification, cancellationToken);
    }

    public async Task<List<Universe>> GetAllOrderedByNameAsync(CancellationToken cancellationToken = default)
    {
        var specification = new AllUniversesOrderedByNameSpecification();
        return await GetBySpecificationAsync(specification, cancellationToken);
    }
}
