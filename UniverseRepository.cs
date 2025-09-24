using Genisis.Core.Data;
using Genisis.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Genisis.Core.Repositories;

public class UniverseRepository : IUniverseRepository
{
    private readonly GenesisDbContext _dbContext;

    public UniverseRepository(GenesisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Universe> AddAsync(Universe universe)
    {
        await _dbContext.Universes.AddAsync(universe);
        await _dbContext.SaveChangesAsync();
        return universe;
    }

    public Task<List<Universe>> GetAllAsync() => _dbContext.Universes.ToListAsync();

    public Task<Universe?> GetByIdWithStoriesAsync(int id)
    {
        return _dbContext.Universes.Include(u => u.Stories).FirstOrDefaultAsync(u => u.Id == id);
    }
}