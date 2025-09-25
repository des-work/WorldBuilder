﻿﻿﻿using Genisis.Core.Models;
using Genisis.Core.Repositories;
using Genisis.App.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Genisis.App.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IUniverseRepository _universeRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;

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
            // Asynchronously load children and set the active view model
            HandleSelectionChangedAsync(value);
        }
    }

    private async void HandleSelectionChangedAsync(object? selectedItem)
    {
        switch (selectedItem)
        {
            case Universe selectedUniverse:
                await LoadStoriesAsync(selectedUniverse);
                ActiveViewModel = new UniverseViewModel(selectedUniverse);
                break;
            case Story selectedStory:
                await LoadChaptersAsync(selectedStory);
                ActiveViewModel = new StoryViewModel(selectedStory);
                break;
            // We will add Chapter and Character cases later
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

    public ICommand AddUniverseCommand { get; }
    public ICommand AddStoryCommand { get; }
    public ICommand AddChapterCommand { get; }

    public MainViewModel(IUniverseRepository universeRepository, IStoryRepository storyRepository, IChapterRepository chapterRepository)
    {
        _universeRepository = universeRepository;
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;

        AddUniverseCommand = new RelayCommand(async _ => await AddUniverse());
        // A story can only be added if the selected item is a Universe.
        AddStoryCommand = new RelayCommand(async _ => await AddStory(), _ => SelectedItem is Universe);
        // A chapter can only be added if the selected item is a Story.
        AddChapterCommand = new RelayCommand(async _ => await AddChapter(), _ => SelectedItem is Story);
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
                parentUniverse.Stories.Add(addedStory);
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
        if (universe.Stories.Any())
        {
            return;
        }

        var stories = await _storyRepository.GetByUniverseIdAsync(universe.Id);
        // Clear any potential design-time items and add the loaded ones.
        universe.Stories.Clear();
        foreach (var story in stories)
        {
            universe.Stories.Add(story);
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
}