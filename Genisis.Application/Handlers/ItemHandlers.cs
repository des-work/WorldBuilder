using Genisis.Core.Models;
using Genisis.Core.Repositories;
using Genisis.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Genisis.Application.Handlers;

public class ItemHandlerFactory : IItemHandlerFactory
{
    private readonly IUniverseRepository _universeRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly ICharacterRepository _characterRepository;

    public ItemHandlerFactory(IUniverseRepository universeRepository, IStoryRepository storyRepository, IChapterRepository chapterRepository, ICharacterRepository characterRepository)
    {
        _universeRepository = universeRepository;
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _characterRepository = characterRepository;
    }

    public IItemHandler? GetHandler(object? item) => item switch
    {
        Universe => new UniverseHandler(_universeRepository, _storyRepository, _characterRepository),
        Story => new StoryHandler(_storyRepository, _chapterRepository),
        Chapter => new ChapterHandler(_chapterRepository),
        Character => new CharacterHandler(_characterRepository),
        _ => null
    };
}

public class UniverseHandler : IItemHandler
{
    private readonly IUniverseRepository _universeRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly ICharacterRepository _characterRepository;

    public UniverseHandler(IUniverseRepository u, IStoryRepository s, ICharacterRepository c)
        => (_universeRepository, _storyRepository, _characterRepository) = (u, s, c);

    public async Task<Story?> AddStoryAsync(Universe parentUniverse, string storyName)
    {
        var newStory = new Story { Name = storyName, UniverseId = parentUniverse.Id };
        var addedStory = await _storyRepository.AddAsync(newStory);
        return addedStory;
    }

    public async Task DeleteAsync(Universe universe)
    {
        await _universeRepository.DeleteAsync(universe);
    }
}

public class StoryHandler : IItemHandler
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;

    public StoryHandler(IStoryRepository s, IChapterRepository c) => (_storyRepository, _chapterRepository) = (s, c);

    public async Task<Chapter?> AddChapterAsync(Story parentStory, string chapterTitle)
    {
        var newChapter = new Chapter { Title = chapterTitle, StoryId = parentStory.Id };
        newChapter.ChapterOrder = (parentStory.Chapters.Any() ? parentStory.Chapters.Max(c => c.ChapterOrder) : 0) + 1;
        var addedChapter = await _chapterRepository.AddAsync(newChapter);
        return addedChapter;
    }

    public async Task DeleteAsync(Story story)
    {
        await _storyRepository.DeleteAsync(story);
    }
}

public class ChapterHandler : IItemHandler
{
    private readonly IChapterRepository _chapterRepository;
    public ChapterHandler(IChapterRepository repo) => _chapterRepository = repo;

    public Task AddChildAsync(object parent) => Task.CompletedTask; // Chapters have no children to add

    public async Task DeleteAsync(Chapter chapter)
    {
        await _chapterRepository.DeleteAsync(chapter);
    }
}

public class CharacterHandler : IItemHandler
{
    private readonly ICharacterRepository _characterRepository;
    public CharacterHandler(ICharacterRepository repo) => _characterRepository = repo;

    public Task AddChildAsync(object parent) => Task.CompletedTask; // Characters have no children to add

    public async Task DeleteAsync(Character character)
    {
        await _characterRepository.DeleteAsync(character);
    }
}
