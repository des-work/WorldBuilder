﻿﻿﻿using Genisis.Core.Models;
using Genisis.Core.Repositories;
using Genisis.App.Services;
using Genisis.App.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace Genisis.App.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IUniverseRepository _universeRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly IPromptGenerationService _promptGenerationService;
    private readonly IItemHandlerFactory _itemHandlerFactory;

    public ObservableCollection<Universe> Universes { get; } = new();

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
            // Asynchronously load children and set the active view model
            UpdateAiContext(value);
            HandleSelectionChangedAsync(value);
        }
    }

    private async void HandleSelectionChangedAsync(object? selectedItem)
    {
        switch (selectedItem)
        {
            case Universe selectedUniverse:
                await LoadStoriesAsync(selectedUniverse);
                await LoadCharactersAsync(selectedUniverse);
                ActiveViewModel = new UniverseViewModel(selectedUniverse, _universeRepository);
                break;
            case Story selectedStory:
                await LoadChaptersAsync(selectedStory);
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

    public ICommand AddUniverseCommand { get; }
    public ICommand AddStoryCommand { get; }
    public ICommand AddChapterCommand { get; }
    public ICommand AddCharacterCommand { get; }
    public ICommand DeleteCommand { get; }

    public MainViewModel(IUniverseRepository universeRepository, IStoryRepository storyRepository, IChapterRepository chapterRepository, ICharacterRepository characterRepository, AIViewModel aiViewModel, IPromptGenerationService promptGenerationService, IItemHandlerFactory itemHandlerFactory)
    {
        _universeRepository = universeRepository;
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _characterRepository = characterRepository;
        AiViewModel = aiViewModel;
        _promptGenerationService = promptGenerationService;
        _itemHandlerFactory = itemHandlerFactory;

        AddUniverseCommand = new RelayCommand(async _ => await AddUniverse());
        // These commands now delegate to the appropriate handler
        AddStoryCommand = new RelayCommand(async _ => await AddChildItemAsync(), _ => SelectedItem is Universe);
        AddChapterCommand = new RelayCommand(async _ => await AddChildItemAsync(), _ => SelectedItem is Story);
        AddCharacterCommand = new RelayCommand(async _ => await AddCharacter(), _ => SelectedItem is Universe or CharacterFolderViewModel);
        DeleteCommand = new RelayCommand(async _ => await DeleteSelectedItemAsync(), _ => SelectedItem is not null);
    }

    private async void UpdateAiContext(object? selectedItem)
    {
        string title;
        string systemPrompt;

        switch (selectedItem)
        {
            case Universe u: title = "Ask Your Universe"; systemPrompt = _promptGenerationService.GenerateUniversePrompt(u); break;
            case Story s: title = $"Ask About '{s.Name}'"; systemPrompt = _promptGenerationService.GenerateStoryPrompt(s); break;
            case Character ch: title = $"Speak with {ch.Name}"; systemPrompt = await _promptGenerationService.GenerateCharacterPromptAsync(ch); break;
            default:
                title = "Ask Your Universe";
                systemPrompt = "You are a helpful AI assistant. There is no specific item selected, so you have no context about the user's world.";
                break;
        }

        AiViewModel.UpdateInteraction(title, systemPrompt);
    }

    private async Task AddChildItemAsync()
    {
        var handler = _itemHandlerFactory.GetHandler(SelectedItem);
        if (handler is not null && SelectedItem is not null)
        {
            await handler.AddChildAsync(SelectedItem);
        }
    }

    private async Task AddCharacter()
    {
        Universe? parentUniverse = SelectedItem switch
        {
            Universe u => u,
            CharacterFolderViewModel cf => cf.ParentUniverse,
            _ => null
        };

        var handler = _itemHandlerFactory.GetHandler(parentUniverse is null ? SelectedItem : parentUniverse);
        if (handler is not null && SelectedItem is not null)
        {
            await handler.AddChildAsync(SelectedItem);
        }
    }

    private async Task DeleteSelectedItemAsync()
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

        var result = MessageBox.Show($"Are you sure you want to permanently delete the {typeName} '{itemName}'?",
                                     "Confirm Deletion",
                                     MessageBoxButton.YesNo,
                                     MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        if (SelectedItem is CharacterFolderViewModel)
        {
            MessageBox.Show("The 'Characters' folder cannot be deleted.", "Action Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var handler = _itemHandlerFactory.GetHandler(SelectedItem);
        if (handler is not null)
        {
            await handler.DeleteAsync(SelectedItem, this);
        }

        // Clear the selection after deletion
        SelectedItem = null;
    }

    public async Task LoadInitialDataAsync()
    {
        var universes = await _universeRepository.GetAllAsync();
        Universes.Clear();
        foreach (var universe in universes)
        {
            Universes.Add(universe);
        }
    }

    private async Task LoadStoriesAsync(Universe universe)
    {
        // Only load if the stories haven't been loaded yet.
        if (universe.Items.OfType<Story>().Any())
        {
            return;
        }

        var stories = await _storyRepository.GetByUniverseIdAsync(universe.Id);
        // The Stories collection on the model is managed by Entity Framework.
        // We only need to populate the UI collection.
        // We clear it first to prevent duplicates on re-load scenarios.
        universe.Items.Clear();
        foreach (var story in stories)
        {
            universe.Stories.Add(story);
            universe.Items.Add(story);
        }
    }

    private async Task LoadChaptersAsync(Story story)
    {
        // Only load if chapters haven't been loaded yet.
        if (story.Chapters.Any())
        {
            return;
        }

        var chapters = await _chapterRepository.GetByStoryIdAsync(story.Id);
        story.Chapters.Clear();
        foreach (var chapter in chapters)
        {
            story.Chapters.Add(chapter);
        }
    }

    private async Task LoadCharactersAsync(Universe universe)
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

        var characters = await _characterRepository.GetByUniverseIdAsync(universe.Id);
        // The Characters collection on the model is managed by EF. We populate the folder's collection for the UI.
        characters.ForEach(c => folder.Characters.Add(c));
    }

    // This method is now only for adding the top-level Universe
    private async Task AddUniverse()
    {
        var dialog = new InputDialog("Enter the name for the new universe:", "New Universe");
        if (dialog.ShowDialog() == true)
        {
            var newUniverse = new Universe { Name = dialog.ResponseText };
            var addedUniverse = await _universeRepository.AddAsync(newUniverse);
            Universes.Add(addedUniverse);
            SelectedItem = addedUniverse; // Select the new item
        }
    }
}