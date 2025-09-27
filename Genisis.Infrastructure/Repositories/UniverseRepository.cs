using Genisis.Core.Models;
using Genisis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

    public async Task<Universe> AddAsync(Universe universe)
    {
        _logger.LogInformation("Adding new Universe: {UniverseName}", universe.Name);
        await _dbContext.Universes.AddAsync(universe);
        await _dbContext.SaveChangesAsync();
        return universe;
    }

    public Task<List<Universe>> GetAllAsync()
    {
        _logger.LogInformation("Getting all Universes.");
        return _dbContext.Universes.ToListAsync();
    }

    public Task<Universe?> GetByIdAsync(int id)
    {
        return _dbContext.Universes.FindAsync(id).AsTask();
    }

    public Task<Universe?> GetByIdWithStoriesAsync(int id)
    {
        _logger.LogInformation("Getting Universe {UniverseId} with its Stories.", id);
        return _dbContext.Universes
            .Include(u => u.Stories)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public Task UpdateAsync(Universe entity)
    {
        _dbContext.Universes.Update(entity);
        return _dbContext.SaveChangesAsync();
    }

    public Task DeleteAsync(Universe entity)
    {
        _dbContext.Universes.Remove(entity);
        return _dbContext.SaveChangesAsync();
    }
}
