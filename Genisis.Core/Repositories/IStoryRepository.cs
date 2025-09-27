using Genisis.Core.Models;

namespace Genisis.Core.Repositories;

public interface IStoryRepository : IRepository<Story>
{
    Task<List<Story>> GetByUniverseIdAsync(int universeId);
}
