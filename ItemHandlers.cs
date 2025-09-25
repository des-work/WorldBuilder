using Genisis.App.Services;
using Genisis.App.ViewModels;
using Genisis.App.Views;
using Genisis.Core.Models;
using Genisis.Core.Repositories;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Genisis.App.Implementation;

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
        CharacterFolderViewModel => new CharacterFolderHandler(_characterRepository),
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

    public async Task AddChildAsync(object parentItem)
    {
        // For a Universe, we can add a Story.
        if (parentItem is not Universe parentUniverse) return;

        var dialog = new InputDialog($"Enter the name for the new story in '{parentUniverse.Name}':", "New Story");
        if (dialog.ShowDialog() != true) return;

        var newStory = new Story { Name = dialog.ResponseText, UniverseId = parentUniverse.Id };
        var addedStory = await _storyRepository.AddAsync(newStory);
        parentUniverse.Items.Add(addedStory);
    }

    public async Task DeleteAsync(object item, MainViewModel vm)
    {
        if (item is not Universe universeToDelete) return;
        await _universeRepository.DeleteAsync(universeToDelete);
        vm.Universes.Remove(universeToDelete);
    }
}

public class StoryHandler : IItemHandler
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;

    public StoryHandler(IStoryRepository s, IChapterRepository c) => (_storyRepository, _chapterRepository) = (s, c);

    public async Task AddChildAsync(object parentItem)
    {
        if (parentItem is not Story parentStory) return;

        var dialog = new InputDialog($"Enter the title for the new chapter in '{parentStory.Name}':", "New Chapter");
        if (dialog.ShowDialog() != true) return;

        var newChapter = new Chapter { Title = dialog.ResponseText, StoryId = parentStory.Id };
        newChapter.ChapterOrder = (parentStory.Chapters.Any() ? parentStory.Chapters.Max(c => c.ChapterOrder) : 0) + 1;
        var addedChapter = await _chapterRepository.AddAsync(newChapter);
        parentStory.Chapters.Add(addedChapter);
    }

    public async Task DeleteAsync(object item, MainViewModel vm)
    {
        if (item is not Story storyToDelete) return;
        await _storyRepository.DeleteAsync(storyToDelete);
        var parentUniverse = vm.Universes.FirstOrDefault(u => u.Id == storyToDelete.UniverseId);
        parentUniverse?.Items.Remove(storyToDelete);
    }
}

// Simplified handlers for Chapter and Character as they don't add children
public class ChapterHandler : IItemHandler
{
    private readonly IChapterRepository _chapterRepository;
    public ChapterHandler(IChapterRepository repo) => _chapterRepository = repo;
    public Task AddChildAsync(object parent) => Task.CompletedTask; // Chapters have no children to add
    public async Task DeleteAsync(object item, MainViewModel vm)
    {
        if (item is not Chapter chapterToDelete) return;
        await _chapterRepository.DeleteAsync(chapterToDelete);
        var parentStory = vm.Universes.SelectMany(u => u.Stories).FirstOrDefault(s => s.Id == chapterToDelete.StoryId);
        parentStory?.Chapters.Remove(chapterToDelete);
    }
}

public class CharacterHandler : IItemHandler
{
    private readonly ICharacterRepository _characterRepository;
    public CharacterHandler(ICharacterRepository repo) => _characterRepository = repo;
    public Task AddChildAsync(object parent) => Task.CompletedTask; // Characters have no children to add
    public async Task DeleteAsync(object item, MainViewModel vm)
    {
        if (item is not Character characterToDelete) return;
        await _characterRepository.DeleteAsync(characterToDelete);
        var parentUniverse = vm.Universes.FirstOrDefault(u => u.Id == characterToDelete.UniverseId);
        var folder = parentUniverse?.Items.OfType<CharacterFolderViewModel>().FirstOrDefault();
        folder?.Characters.Remove(characterToDelete);
    }
}

public class CharacterFolderHandler : IItemHandler
{
    private readonly ICharacterRepository _characterRepository;
    public CharacterFolderHandler(ICharacterRepository repo) => _characterRepository = repo;

    public async Task AddChildAsync(object parentItem)
    {
        if (parentItem is not CharacterFolderViewModel folder) return;
        var parentUniverse = folder.ParentUniverse;

        var dialog = new InputDialog($"Enter the name for the new character in '{parentUniverse.Name}':", "New Character");
        if (dialog.ShowDialog() != true) return;

        var newCharacter = new Character { Name = dialog.ResponseText, UniverseId = parentUniverse.Id };
        var addedCharacter = await _characterRepository.AddAsync(newCharacter);
        folder.Characters.Add(addedCharacter);
    }

    public Task DeleteAsync(object item, MainViewModel vm) => Task.CompletedTask; // Folder cannot be deleted
}