using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Genisis.Core.Services;

public interface IAiService
{
    Task<List<string>> GetLocalModelsAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> StreamCompletionAsync(string model, string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default);
}


