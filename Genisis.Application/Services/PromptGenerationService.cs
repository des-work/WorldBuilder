using Genisis.Core.Models;
using Genisis.Core.Repositories;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genisis.Application.Services;

public class PromptGenerationService : IPromptGenerationService
{
    private readonly IUniverseRepository _universeRepository;
    private readonly IChapterRepository _chapterRepository;

    public PromptGenerationService(IUniverseRepository universeRepository, IChapterRepository chapterRepository)
    {
        _universeRepository = universeRepository;
        _chapterRepository = chapterRepository;
    }

    public string GenerateUniversePrompt(Universe universe)
    {
        return $"You are a world-building assistant. Your knowledge is based on the following context about the '{universe.Name}' universe. Answer the user's questions based on this information.\n\nCONTEXT:\nUniverse Name: {universe.Name}\nDescription: {universe.Description}";
    }

    public string GenerateStoryPrompt(Story story)
    {
        return $"You are a world-building assistant. Your knowledge is based on the following context about the story '{story.Name}'. Answer the user's questions based on this information.\n\nCONTEXT:\nStory Name: {story.Name}\nLogline: {story.Logline}";
    }

    public async Task<string> GenerateCharacterPromptAsync(Character character, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"You are to role-play as the character '{character.Name}'. Speak in their voice and embody their personality. Do not break character. Do not mention that you are an AI. Respond directly to the user's message as if you are {character.Name}.");
        sb.AppendLine("\n---");
        sb.AppendLine("CHARACTER BIO:");
        sb.AppendLine($"Name: {character.Name}");
        sb.AppendLine($"Tier: {character.Tier}");
        sb.AppendLine($"Bio: {character.Bio}");
        if (!string.IsNullOrWhiteSpace(character.Notes))
        {
            sb.AppendLine($"Private Notes (for your personality): {character.Notes}");
        }
        sb.AppendLine("\n---");

        var parentUniverse = await _universeRepository.GetByIdAsync(character.UniverseId, cancellationToken);
        if (parentUniverse != null)
        {
            sb.AppendLine("WORLD KNOWLEDGE (The universe you live in):");
            sb.AppendLine(parentUniverse.Description);
            sb.AppendLine("\n---");
        }

        // Find all chapters this character appears in to provide event context.
        var chapters = await _chapterRepository.GetByCharacterIdAsync(character.Id, cancellationToken);
        if (chapters.Any())
        {
            sb.AppendLine("RECENT EVENTS (Things you have experienced or are aware of):");
            foreach (var chapter in chapters.OrderBy(c => c.ChapterOrder))
            {
                sb.AppendLine($"- In the chapter titled '{chapter.Title}', the following happened: {chapter.Content}");
            }
            sb.AppendLine("\n---");
        }

        return sb.ToString();
    }
}
