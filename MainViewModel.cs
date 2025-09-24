using Genisis.Core.Models;
using Genisis.Core.Repositories;
using System.Collections.ObjectModel;
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
            // Handle selection logic based on the type of the selected item
            if (value is Universe selectedUniverse)
            {
                LoadStoriesAsync(selectedUniverse);
                ActiveViewModel = new UniverseViewModel(selectedUniverse);
            }
            else if (value is Story selectedStory)
            {
                LoadChaptersAsync(selectedStory);
                ActiveViewModel = new StoryViewModel(selectedStory);
            }
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

    public ObservableCollection<Story> Stories { get; } = new();
    public ObservableCollection<Chapter> Chapters { get; } = new();

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
        Stories.Clear();
        if (universe is not null)
        {
            var stories = await _storyRepository.GetByUniverseIdAsync(universe.Id);
            foreach (var story in stories)
            {
                Stories.Add(story);
            }
        }
    }

    private async Task LoadChaptersAsync(Story story)
    {
        Chapters.Clear();
        if (story is not null)
        {
            var chapters = await _chapterRepository.GetByStoryIdAsync(story.Id);
            foreach (var chapter in chapters)
            {
                // This is inefficient. We will improve this later.
                story.Chapters.Add(chapter);
            }
        }
    }
}