using Genisis.Core.Models;
using System.Threading;

namespace Genisis.Core.Repositories;

public interface IChapterRepository : IRepository<Chapter>
{
    Task<List<Chapter>> GetByStoryIdAsync(int storyId, CancellationToken cancellationToken = default);
    Task<List<Chapter>> GetByCharacterIdAsync(int characterId, CancellationToken cancellationToken = default);
}
