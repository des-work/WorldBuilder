using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genisis.App.Services;

public interface IAiService
{
    Task<List<string>> GetLocalModelsAsync();
    Task<string> GetCompletionAsync(string model, string prompt);
}