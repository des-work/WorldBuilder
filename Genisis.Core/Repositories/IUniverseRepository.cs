using Genisis.Core.Models;
using System.Threading;

namespace Genisis.Core.Repositories;

public interface IUniverseRepository : IRepository<Universe>
{
    Task<Universe?> GetByIdWithStoriesAsync(int id, CancellationToken cancellationToken = default);
}
