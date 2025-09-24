using Genisis.Core.Data;
using Genisis.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Genisis.Core.Repositories;

public class ChapterRepository : IChapterRepository
{
    private readonly GenesisDbContext _dbContext;
    private readonly ILogger<ChapterRepository> _logger;

    public ChapterRepository(GenesisDbContext dbContext, ILogger<ChapterRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Chapter> AddAsync(Chapter entity)
    {
        _logger.LogInformation("Adding new Chapter: {ChapterTitle}", entity.Title);
        await _dbContext.Chapters.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public Task<List<Chapter>> GetAllAsync()
    {
        return _dbContext.Chapters.ToListAsync();
    }

    public Task<Chapter?> GetByIdAsync(int id)
    {
        return _dbContext.Chapters.FindAsync(id).AsTask();
    }

    public Task<List<Chapter>> GetByStoryIdAsync(int storyId)
    {
        _logger.LogInformation("Getting all chapters for StoryId: {StoryId}", storyId);
        return _dbContext.Chapters.Where(c => c.StoryId == storyId).OrderBy(c => c.ChapterOrder).ToListAsync();
    }

    public Task UpdateAsync(Chapter entity)
    {
        _dbContext.Chapters.Update(entity);
        return _dbContext.SaveChangesAsync();
    }

    public Task DeleteAsync(Chapter entity)
    {
        _dbContext.Chapters.Remove(entity);
        return _dbContext.SaveChangesAsync();
    }
}