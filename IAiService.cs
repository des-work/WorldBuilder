using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genisis.App.Services;

public interface IAiService
{
    Task<List<string>> GetLocalModelsAsync();
    IAsyncEnumerable<string> StreamCompletionAsync(string model, string prompt);
}