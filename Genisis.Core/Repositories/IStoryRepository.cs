using Genisis.Core.Models;
using System.Threading;

namespace Genisis.Core.Repositories;

public interface IStoryRepository : IRepository<Story>
{
    Task<List<Story>> GetByUniverseIdAsync(int universeId, CancellationToken cancellationToken = default);
}
