using Genisis.Core.Models;
using Genisis.Core.Repositories;
using Genisis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

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

    public async Task<Story> AddAsync(Story entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new Story: {StoryName}", entity.Name);
        await _dbContext.Stories.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public Task<List<Story>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Story?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task<List<Story>> GetByUniverseIdAsync(int universeId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all stories for UniverseId: {UniverseId}", universeId);
        return _dbContext.Stories
            .AsNoTracking()
            .Where(s => s.UniverseId == universeId)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(Story entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Stories.Update(entity);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteAsync(Story entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Stories.Remove(entity);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
