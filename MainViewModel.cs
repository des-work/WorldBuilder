﻿﻿﻿using Genisis.Core.Models;
using Genisis.Core.Repositories;
using Genisis.App.Services;
using Genisis.App.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace Genisis.App.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IUniverseRepository _universeRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly ICharacterRepository _characterRepository;

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

    public MainViewModel(IUniverseRepository universeRepository, IStoryRepository storyRepository, IChapterRepository chapterRepository, ICharacterRepository characterRepository, AIViewModel aiViewModel)
    {
        _universeRepository = universeRepository;
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _characterRepository = characterRepository;
        AiViewModel = aiViewModel;

        AddUniverseCommand = new RelayCommand(async _ => await AddUniverse());
        // A story can only be added if the selected item is a Universe.
        AddStoryCommand = new RelayCommand(async _ => await AddStory(), _ => SelectedItem is Universe);
        // A chapter can only be added if the selected item is a Story.
        AddChapterCommand = new RelayCommand(async _ => await AddChapter(), _ => SelectedItem is Story);
        // A character can be added if a Universe or a CharacterFolder is selected.
        AddCharacterCommand = new RelayCommand(async _ => await AddCharacter(), _ => SelectedItem is Universe or CharacterFolderViewModel);
        // An item can be deleted if something is selected.
        DeleteCommand = new RelayCommand(async _ => await DeleteSelectedItemAsync(), _ => SelectedItem is not null);
    }

    private void UpdateAiContext(object? selectedItem)
    {
        string title;
        string systemPrompt;

        switch (selectedItem)
        {
            case Universe u:
                title = "Ask Your Universe";
                systemPrompt = $"You are a world-building assistant. Your knowledge is based on the following context about the '{u.Name}' universe. Answer the user's questions based on this information.\n\nCONTEXT:\nUniverse Name: {u.Name}\nDescription: {u.Description}";
                break;
            case Story s:
                title = $"Ask About '{s.Name}'";
                systemPrompt = $"You are a world-building assistant. Your knowledge is based on the following context about the story '{s.Name}'. Answer the user's questions based on this information.\n\nCONTEXT:\nStory Name: {s.Name}\nLogline: {s.Logline}";
                break;
            case Character ch:
                title = $"Speak with {ch.Name}";
                systemPrompt = $"You are to role-play as the character '{ch.Name}'. Speak in their voice and embody their personality based on the following information. Do not break character. Do not mention that you are an AI. Respond directly to the user's message as if you are {ch.Name}.\n\nCHARACTER BIO:\nName: {ch.Name}\nTier: {ch.Tier}\nBio: {ch.Bio}";
                break;
            default:
                title = "Ask Your Universe";
                systemPrompt = "You are a helpful AI assistant. There is no specific item selected, so you have no context about the user's world.";
                break;
        }

        AiViewModel.UpdateInteraction(title, systemPrompt);
    }

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

    private async Task AddStory()
    {
        if (SelectedItem is Universe parentUniverse)
        {
            var dialog = new InputDialog($"Enter the name for the new story in '{parentUniverse.Name}':", "New Story");
            if (dialog.ShowDialog() == true)
            {
                var newStory = new Story { Name = dialog.ResponseText, UniverseId = parentUniverse.Id };
                var addedStory = await _storyRepository.AddAsync(newStory);
                // No need to add to parentUniverse.Stories; EF handles the relationship.
                parentUniverse.Items.Add(addedStory);   // Add to UI collection
                SelectedItem = addedStory; // Select the new item
            }
        }
    }

    private async Task AddChapter()
    {
        if (SelectedItem is Story parentStory)
        {
            var dialog = new InputDialog($"Enter the title for the new chapter in '{parentStory.Name}':", "New Chapter");
            if (dialog.ShowDialog() == true)
            {
                var newChapter = new Chapter { Title = dialog.ResponseText, StoryId = parentStory.Id };
                // Basic logic to set the chapter order
                newChapter.ChapterOrder = (parentStory.Chapters.Any() ? parentStory.Chapters.Max(c => c.ChapterOrder) : 0) + 1;
                var addedChapter = await _chapterRepository.AddAsync(newChapter);
                parentStory.Chapters.Add(addedChapter);
                SelectedItem = addedChapter; // Select the new item
            }
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

        if (parentUniverse is not null)
        {
            var dialog = new InputDialog($"Enter the name for the new character in '{parentUniverse.Name}':", "New Character");
            if (dialog.ShowDialog() == true)
            {
                var newCharacter = new Character { Name = dialog.ResponseText, UniverseId = parentUniverse.Id };
                var addedCharacter = await _characterRepository.AddAsync(newCharacter);
                // We need to find the folder to add the character to.
                // No need to add to parentUniverse.Characters; EF handles the relationship.
                var folder = parentUniverse.Items.OfType<CharacterFolderViewModel>().FirstOrDefault();
                folder?.Characters.Add(addedCharacter);
                SelectedItem = addedCharacter;
            }
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

        switch (SelectedItem)
        {
            case Universe universeToDelete:
                await _universeRepository.DeleteAsync(universeToDelete);
                Universes.Remove(universeToDelete);
                break;

            case Story storyToDelete:
                await _storyRepository.DeleteAsync(storyToDelete);
                // Find the parent universe to remove the story from its collection
                var parentUniverse = Universes.FirstOrDefault(u => u.Id == storyToDelete.UniverseId);
                // Remove from the UI collection
                parentUniverse?.Items.Remove(storyToDelete);
                break;

            case Chapter chapterToDelete:
                await _chapterRepository.DeleteAsync(chapterToDelete);
                // Find the parent story to remove the chapter from its collection
                Story? parentStory = null;
                foreach (var u in Universes)
                {
                    parentStory = u.Stories.FirstOrDefault(s => s.Chapters.Contains(chapterToDelete));
                    if (parentStory != null) break;
                }
                parentStory?.Chapters.Remove(chapterToDelete);
                break;

            case Character characterToDelete:
                await _characterRepository.DeleteAsync(characterToDelete);
                // Find the parent universe and then its character folder to remove the character
                var parentUni = Universes.FirstOrDefault(u => u.Id == characterToDelete.UniverseId);
                var folder = parentUni?.Items.OfType<CharacterFolderViewModel>().FirstOrDefault();
                folder?.Characters.Remove(characterToDelete);
                break;
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
}