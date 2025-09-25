﻿using Genisis.Core.Models;
using Genisis.Core.Repositories;
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

    public MainViewModel(IUniverseRepository universeRepository, IStoryRepository storyRepository, IChapterRepository chapterRepository)
    {
        _universeRepository = universeRepository;
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;

        AddUniverseCommand = new RelayCommand(async _ => await AddUniverse());
    }

    private async Task AddUniverse()
    {
        // This is a placeholder. In a real implementation, we would open a dialog
        // to get the name from the user.
        var newUniverse = new Universe { Name = "New Universe" };
        var addedUniverse = await _universeRepository.AddAsync(newUniverse);
        Universes.Add(addedUniverse);
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