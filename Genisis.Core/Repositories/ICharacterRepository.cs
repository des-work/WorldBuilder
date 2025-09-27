using Genisis.Core.Models;

namespace Genisis.Core.Repositories;

public interface ICharacterRepository : IRepository<Character>
{
    Task<List<Character>> GetByUniverseIdAsync(int universeId);
}
