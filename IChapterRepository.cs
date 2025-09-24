using Genisis.Core.Models;

namespace Genisis.Core.Repositories;

public interface IChapterRepository : IRepository<Chapter>
{
    Task<List<Chapter>> GetByStoryIdAsync(int storyId);
}