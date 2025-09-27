using Genisis.Core.Models;
using Genisis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Genisis.Infrastructure.Repositories;

public class StoryRepository : IStoryRepository
{
    private readonly GenesisDbContext _dbContext;
    private readonly ILogger<StoryRepository> _logger;

    public StoryRepository(GenesisDbContext dbContext, ILogger<StoryRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Story> AddAsync(Story entity)
    {
        _logger.LogInformation("Adding new Story: {StoryName}", entity.Name);
        await _dbContext.Stories.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public Task<List<Story>> GetAllAsync()
    {
        return _dbContext.Stories.ToListAsync();
    }

    public Task<Story?> GetByIdAsync(int id)
    {
        return _dbContext.Stories.FindAsync(id).AsTask();
    }

    public Task<List<Story>> GetByUniverseIdAsync(int universeId)
    {
        _logger.LogInformation("Getting all stories for UniverseId: {UniverseId}", universeId);
        return _dbContext.Stories.Where(s => s.UniverseId == universeId).ToListAsync();
    }

    public Task UpdateAsync(Story entity)
    {
        _dbContext.Stories.Update(entity);
        return _dbContext.SaveChangesAsync();
    }

    public Task DeleteAsync(Story entity)
    {
        _dbContext.Stories.Remove(entity);
        return _dbContext.SaveChangesAsync();
    }
}
