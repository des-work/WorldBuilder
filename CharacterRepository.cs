using Genisis.Core.Data;
using Genisis.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Genisis.Core.Repositories;

public class CharacterRepository : ICharacterRepository
{
    private readonly GenesisDbContext _dbContext;
    private readonly ILogger<CharacterRepository> _logger;

    public CharacterRepository(GenesisDbContext dbContext, ILogger<CharacterRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Character> AddAsync(Character entity)
    {
        _logger.LogInformation("Adding new Character: {CharacterName}", entity.Name);
        await _dbContext.Characters.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public Task<List<Character>> GetAllAsync()
    {
        return _dbContext.Characters.ToListAsync();
    }

    public Task<Character?> GetByIdAsync(int id)
    {
        return _dbContext.Characters.FindAsync(id).AsTask();
    }

    public Task<List<Character>> GetByUniverseIdAsync(int universeId)
    {
        _logger.LogInformation("Getting all characters for UniverseId: {UniverseId}", universeId);
        return _dbContext.Characters.Where(c => c.UniverseId == universeId).ToListAsync();
    }

    public Task UpdateAsync(Character entity)
    {
        _dbContext.Characters.Update(entity);
        return _dbContext.SaveChangesAsync();
    }

    public Task DeleteAsync(Character entity)
    {
        _dbContext.Characters.Remove(entity);
        return _dbContext.SaveChangesAsync();
    }
}