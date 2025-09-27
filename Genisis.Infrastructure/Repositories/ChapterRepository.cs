using Genisis.Core.Models;
using Genisis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Genisis.Infrastructure.Repositories;

public class ChapterRepository : IChapterRepository
{
    private readonly GenesisDbContext _dbContext;
    private readonly ILogger<ChapterRepository> _logger;

    public ChapterRepository(GenesisDbContext dbContext, ILogger<ChapterRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Chapter> AddAsync(Chapter entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new Chapter: {ChapterTitle}", entity.Title);
        await _dbContext.Chapters.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public Task<List<Chapter>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Chapters
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Chapter?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Chapters
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<List<Chapter>> GetByStoryIdAsync(int storyId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all chapters for StoryId: {StoryId}", storyId);
        return _dbContext.Chapters
            .AsNoTracking()
            .Where(c => c.StoryId == storyId)
            .OrderBy(c => c.ChapterOrder)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Chapter>> GetByCharacterIdAsync(int characterId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all chapters for CharacterId: {CharacterId}", characterId);
        return _dbContext.Chapters
            .AsNoTracking()
            .Where(c => c.Characters.Any(ch => ch.Id == characterId))
            .OrderBy(c => c.ChapterOrder)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(Chapter entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Chapters.Update(entity);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteAsync(Chapter entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Chapters.Remove(entity);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
