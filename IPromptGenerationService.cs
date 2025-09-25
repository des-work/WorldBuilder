using Genisis.Core.Models;
using System.Threading.Tasks;

namespace Genisis.App.Services;

public interface IPromptGenerationService
{
    Task<string> GenerateCharacterPromptAsync(Character character);
    string GenerateUniversePrompt(Universe universe);
    string GenerateStoryPrompt(Story story);
}