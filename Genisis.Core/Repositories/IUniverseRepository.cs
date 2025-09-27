using Genisis.Core.Models;

namespace Genisis.Core.Repositories;

public interface IUniverseRepository : IRepository<Universe>
{
    Task<Universe?> GetByIdWithStoriesAsync(int id);
}
