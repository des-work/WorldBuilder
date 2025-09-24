using Genisis.Core.Models;

namespace Genisis.Core.Repositories;

public interface IUniverseRepository
{
    Task<Universe> AddAsync(Universe universe);
    Task<List<Universe>> GetAllAsync();
    Task<Universe?> GetByIdWithStoriesAsync(int id);
}