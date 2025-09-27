using Genisis.Core.Models;
using Genisis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Genisis.Infrastructure.Repositories;

public class CharacterRepository : ICharacterRepository
{
    private readonly GenesisDbContext _dbContext;
    private readonly ILogger<CharacterRepository> _logger;

    public CharacterRepository(GenesisDbContext dbContext, ILogger<CharacterRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Character> AddAsync(Character entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new Character: {CharacterName}", entity.Name);
        await _dbContext.Characters.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public Task<List<Character>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Characters
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Character?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Characters
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<List<Character>> GetByUniverseIdAsync(int universeId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all characters for UniverseId: {UniverseId}", universeId);
        return _dbContext.Characters
            .AsNoTracking()
            .Where(c => c.UniverseId == universeId)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(Character entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Characters.Update(entity);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteAsync(Character entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Characters.Remove(entity);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
