using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Genisis.Core.Models;
using Genisis.Core.Repositories;
using Genisis.Application.Services;
using Genisis.Presentation.Wpf.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Genisis.Presentation.Wpf.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IUniverseRepository _universeRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly IPromptGenerationService _promptGenerationService;
    private readonly IDialogService _dialogService;

    public ObservableCollection<Universe> Universes { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    private object? _selectedItem;
    public object? SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            OnPropertyChanged();
            // When selection changes, we need to notify the commands that their
            // CanExecute status might have changed.
            (AddStoryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (AddChapterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (AddCharacterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
            // Update AI context and set the active view model
            _ = UpdateAiContext(value);
            HandleSelectionChanged(value);
        }
    }

    private void HandleSelectionChanged(object? selectedItem)
    {
        switch (selectedItem)
        {
            case Universe selectedUniverse:
                // Don't eagerly load children on selection - only when expanded
                ActiveViewModel = new UniverseViewModel(selectedUniverse, _universeRepository);
                break;
            case Story selectedStory:
                // Don't eagerly load chapters on selection - only when expanded
                ActiveViewModel = new StoryViewModel(selectedStory, _storyRepository);
                break;
            case Character selectedCharacter:
                ActiveViewModel = new CharacterViewModel(selectedCharacter, _characterRepository);
                break;
            case CharacterFolderViewModel:
                ActiveViewModel = null; // Or a dedicated view for the folder
                break;
            case Chapter selectedChapter:
                ActiveViewModel = new ChapterViewModel(selectedChapter, _chapterRepository);
                break;
            default:
                ActiveViewModel = null;
                break;
        }
    }

    private ViewModelBase? _activeViewModel;
    public ViewModelBase? ActiveViewModel
    {
        get => _activeViewModel;
        set
        {
            _activeViewModel = value;
            OnPropertyChanged();
        }
    }

    public AIViewModel AiViewModel { get; }

    public IAsyncRelayCommand AddUniverseCommand { get; }
    public IAsyncRelayCommand AddStoryCommand { get; }
    public IAsyncRelayCommand AddChapterCommand { get; }
    public IAsyncRelayCommand AddCharacterCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }

    public MainViewModel(IUniverseRepository universeRepository, IStoryRepository storyRepository, IChapterRepository chapterRepository, ICharacterRepository characterRepository, AIViewModel aiViewModel, IPromptGenerationService promptGenerationService, IDialogService dialogService)
    {
        _universeRepository = universeRepository;
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _characterRepository = characterRepository;
        AiViewModel = aiViewModel;
        _promptGenerationService = promptGenerationService;
        _dialogService = dialogService;

        AddUniverseCommand = new AsyncRelayCommand(AddUniverse);
        AddStoryCommand = new AsyncRelayCommand(AddStory, CanAddStory);
        AddChapterCommand = new AsyncRelayCommand(AddChapter, CanAddChapter);
        AddCharacterCommand = new AsyncRelayCommand(AddCharacter, CanAddCharacter);
        DeleteCommand = new AsyncRelayCommand(DeleteSelectedItemAsync, CanDelete);
    }

    private bool CanAddStory() => SelectedItem is Universe;
    private bool CanAddChapter() => SelectedItem is Story;
    private bool CanAddCharacter() => SelectedItem is Universe or CharacterFolderViewModel;
    private bool CanDelete() => SelectedItem is not null;

    private async Task UpdateAiContext(object? selectedItem, CancellationToken cancellationToken = default)
    {
        string title;
        string systemPrompt;

        switch (selectedItem)
        {
            case Universe u: title = "Ask Your Universe"; systemPrompt = _promptGenerationService.GenerateUniversePrompt(u); break;
            case Story s: title = $"Ask About '{s.Name}'"; systemPrompt = _promptGenerationService.GenerateStoryPrompt(s); break;
            case Character ch: title = $"Speak with {ch.Name}"; systemPrompt = await _promptGenerationService.GenerateCharacterPromptAsync(ch, cancellationToken); break;
            default:
                title = "Ask Your Universe";
                systemPrompt = "You are a helpful AI assistant. There is no specific item selected, so you have no context about the user's world.";
                break;
        }

        AiViewModel.UpdateInteraction(title, systemPrompt);
    }

    private bool _dataLoaded = false;

    public async Task LoadInitialDataAsync(CancellationToken cancellationToken = default)
    {
        // Only load once
        if (_dataLoaded) return;

        IsLoading = true;
        try
        {
            var universes = await _universeRepository.GetAllAsync(cancellationToken);
            
            // Clear and batch-add universes to minimize ObservableCollection events
            Universes.Clear();
            foreach (var universe in universes)
            {
                // Don't eagerly load children - they'll be loaded on expansion
                Universes.Add(universe);
            }
            _dataLoaded = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadUniverseChildrenAsync(Universe universe, CancellationToken cancellationToken = default)
    {
        await LoadStoriesAsync(universe, cancellationToken);
        await LoadCharactersAsync(universe, cancellationToken);
    }

    public async Task LoadStoryChildrenAsync(Story story, CancellationToken cancellationToken = default)
    {
        await LoadChaptersAsync(story, cancellationToken);
    }

    public async Task LoadCharacterFolderChildrenAsync(CharacterFolderViewModel characterFolder, CancellationToken cancellationToken = default)
    {
        await LoadCharactersAsync(characterFolder.ParentUniverse, cancellationToken);
    }

    private async Task LoadStoriesAsync(Universe universe, CancellationToken cancellationToken = default)
    {
        // Only load if the stories haven't been loaded yet.
        if (universe.Items.OfType<Story>().Any())
        {
            return;
        }

        var stories = await _storyRepository.GetByUniverseIdAsync(universe.Id, cancellationToken);
        
        // Batch UI updates to minimize ObservableCollection events
        var itemsToAdd = new List<object>();
        
        foreach (var story in stories)
        {
            universe.Stories.Add(story);
            itemsToAdd.Add(story);
        }

        // Add all items in one batch
        foreach (var item in itemsToAdd)
        {
            universe.Items.Add(item);
        }
    }

    private async Task LoadChaptersAsync(Story story, CancellationToken cancellationToken = default)
    {
        // Only load if chapters haven't been loaded yet.
        if (story.Chapters.Any())
        {
            return;
        }

        var chapters = await _chapterRepository.GetByStoryIdAsync(story.Id, cancellationToken);
        
        // Batch UI updates to minimize ObservableCollection events
        story.Chapters.Clear();
        foreach (var chapter in chapters)
        {
            story.Chapters.Add(chapter);
        }
    }

    private async Task LoadCharactersAsync(Universe universe, CancellationToken cancellationToken = default)
    {
        var folder = universe.Items.OfType<CharacterFolderViewModel>().FirstOrDefault();
        if (folder is null)
        {
            folder = new CharacterFolderViewModel(universe);
            universe.Items.Add(folder);
        }

        // Only load if the folder is empty.
        if (folder.Characters.Any())
        {
            return;
        }

        var characters = await _characterRepository.GetByUniverseIdAsync(universe.Id, cancellationToken);
        
        // Batch UI updates to minimize ObservableCollection events
        // The Characters collection on the model is managed by EF. We populate the folder's collection for the UI.
        foreach (var character in characters)
        {
            folder.Characters.Add(character);
        }
    }

    private async Task AddUniverse(CancellationToken cancellationToken = default)
    {
        if (_dialogService.ShowInputDialog("Enter the name for the new universe:", "New Universe", out var universeName))
        {
            var newUniverse = new Universe { Name = universeName };
            var addedUniverse = await _universeRepository.AddAsync(newUniverse, cancellationToken);
            Universes.Add(addedUniverse);
            SelectedItem = addedUniverse; // Select the new item
        }
    }

    private async Task AddStory(CancellationToken cancellationToken = default)
    {
        if (SelectedItem is Universe parentUniverse &&
            _dialogService.ShowInputDialog($"Enter the name for the new story in '{parentUniverse.Name}':", "New Story", out var storyName))
        {
            var newStory = new Story { Name = storyName, UniverseId = parentUniverse.Id };
            var addedStory = await _storyRepository.AddAsync(newStory, cancellationToken);
            parentUniverse.Items.Add(addedStory);
        }
    }

    private async Task AddChapter(CancellationToken cancellationToken = default)
    {
        if (SelectedItem is Story parentStory &&
            _dialogService.ShowInputDialog($"Enter the title for the new chapter in '{parentStory.Name}':", "New Chapter", out var chapterTitle))
        {
            var newChapter = new Chapter { Title = chapterTitle, StoryId = parentStory.Id };
            newChapter.ChapterOrder = (parentStory.Chapters.Any() ? parentStory.Chapters.Max(c => c.ChapterOrder) : 0) + 1;
            var addedChapter = await _chapterRepository.AddAsync(newChapter, cancellationToken);
            parentStory.Chapters.Add(addedChapter);
        }
    }

    private async Task AddCharacter(CancellationToken cancellationToken = default)
    {
        Universe? parentUniverse = SelectedItem switch
        {
            Universe u => u,
            CharacterFolderViewModel cf => cf.ParentUniverse,
            _ => null
        };

        if (parentUniverse != null &&
            _dialogService.ShowInputDialog($"Enter the name for the new character in '{parentUniverse.Name}':", "New Character", out var characterName))
        {
            var newCharacter = new Character { Name = characterName, UniverseId = parentUniverse.Id };
            var addedCharacter = await _characterRepository.AddAsync(newCharacter, cancellationToken);

            var folder = parentUniverse.Items.OfType<CharacterFolderViewModel>().FirstOrDefault();
            if (folder != null)
            {
                folder.Characters.Add(addedCharacter);
            }
        }
    }

    private async Task DeleteSelectedItemAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedItem is null) return;

        var typeName = SelectedItem.GetType().Name;
        var itemName = SelectedItem switch
        {
            Universe u => u.Name,
            Story s => s.Name,
            Chapter c => c.Title,
            Character ch => ch.Name,
            CharacterFolderViewModel => "Characters Folder",
            _ => "the selected item"
        };

        if (!_dialogService.ShowConfirmation($"Are you sure you want to permanently delete the {typeName} '{itemName}'?", "Confirm Deletion"))
            return;

        if (SelectedItem is CharacterFolderViewModel)
        {
            _dialogService.ShowMessage("The 'Characters' folder cannot be deleted.", "Action Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        switch (SelectedItem)
        {
            case Universe universe:
                await _universeRepository.DeleteAsync(universe, cancellationToken);
                Universes.Remove(universe);
                break;
            case Story story:
                await _storyRepository.DeleteAsync(story, cancellationToken);
                var parentUniverse = Universes.FirstOrDefault(u => u.Id == story.UniverseId);
                parentUniverse?.Items.Remove(story);
                break;
            case Chapter chapter:
                await _chapterRepository.DeleteAsync(chapter, cancellationToken);
                var parentStory = Universes.SelectMany(u => u.Stories).FirstOrDefault(s => s.Id == chapter.StoryId);
                parentStory?.Chapters.Remove(chapter);
                break;
            case Character character:
                await _characterRepository.DeleteAsync(character, cancellationToken);
                var characterParentUniverse = Universes.FirstOrDefault(u => u.Id == character.UniverseId);
                var characterFolder = characterParentUniverse?.Items.OfType<CharacterFolderViewModel>().FirstOrDefault();
                characterFolder?.Characters.Remove(character);
                break;
        }

        // Clear the selection after deletion
        SelectedItem = null;
    }
}
