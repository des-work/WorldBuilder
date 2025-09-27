using Genisis.Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Genisis.Application.Services;

public interface IPromptGenerationService
{
    string GenerateUniversePrompt(Universe universe);
    string GenerateStoryPrompt(Story story);
    Task<string> GenerateCharacterPromptAsync(Character character, CancellationToken cancellationToken = default);
}
