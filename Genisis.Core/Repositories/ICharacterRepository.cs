using Genisis.Core.Models;
using System.Threading;

namespace Genisis.Core.Repositories;

public interface ICharacterRepository : IRepository<Character>
{
    Task<List<Character>> GetByUniverseIdAsync(int universeId, CancellationToken cancellationToken = default);
}
