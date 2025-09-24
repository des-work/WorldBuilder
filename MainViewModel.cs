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
            }
        }
    }

    public ObservableCollection<Story> Stories { get; } = new();

    public ICommand AddUniverseCommand { get; }

    public MainViewModel(IUniverseRepository universeRepository, IStoryRepository storyRepository)
    {
        _universeRepository = universeRepository;
        _storyRepository = storyRepository;

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
}