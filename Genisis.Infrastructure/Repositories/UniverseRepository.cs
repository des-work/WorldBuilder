using Genisis.Core.Models;
using Genisis.Core.Repositories;
using Genisis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Genisis.Infrastructure.Repositories;

public class UniverseRepository : IUniverseRepository
{
    private readonly GenesisDbContext _dbContext;
    private readonly ILogger<UniverseRepository> _logger;

    public UniverseRepository(GenesisDbContext dbContext, ILogger<UniverseRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Universe> AddAsync(Universe universe, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new Universe: {UniverseName}", universe.Name);
        await _dbContext.Universes.AddAsync(universe, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return universe;
    }

    public Task<List<Universe>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all Universes.");
        return _dbContext.Universes
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Universe?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Universes
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<Universe?> GetByIdWithStoriesAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting Universe {UniverseId} with its Stories.", id);
        return _dbContext.Universes
            .AsNoTracking()
            .Include(u => u.Stories)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task UpdateAsync(Universe entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Universes.Update(entity);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteAsync(Universe entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Universes.Remove(entity);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
