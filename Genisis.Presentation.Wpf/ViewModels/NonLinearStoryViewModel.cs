using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Genisis.Core.Models;
using Genisis.Core.Repositories;
using Genisis.Core.Services;
using Genisis.Presentation.Wpf.Services;

namespace Genisis.Presentation.Wpf.ViewModels;

/// <summary>
/// ViewModel for non-linear story creation with multi-context awareness
/// </summary>
public class NonLinearStoryViewModel : ViewModelBase
{
    private readonly IUniverseRepository _universeRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IAiService _aiService;
    private readonly IDialogService _dialogService;

    private StoryElement? _primaryContext;
    private StoryElement? _secondaryContext;
    private StoryElement? _tertiaryContext;

    public NonLinearStoryViewModel(
        IUniverseRepository universeRepository,
        IStoryRepository storyRepository,
        ICharacterRepository characterRepository,
        IChapterRepository chapterRepository,
        IAiService aiService,
        IDialogService dialogService)
    {
        _universeRepository = universeRepository;
        _storyRepository = storyRepository;
        _characterRepository = characterRepository;
        _chapterRepository = chapterRepository;
        _aiService = aiService;
        _dialogService = dialogService;

        // Initialize commands
        SetPrimaryContextCommand = new RelayCommand<StoryElement>(SetPrimaryContext);
        SetSecondaryContextCommand = new RelayCommand<StoryElement>(SetSecondaryContext);
        ClearContextCommand = new RelayCommand<StoryElement>(ClearContext);
        CreateElementCommand = new RelayCommand<ElementType>(CreateElement);
        LinkElementsCommand = new RelayCommand<object>(LinkElements);
        NavigateToElementCommand = new RelayCommand<StoryElement>(NavigateToElement);
        AddToFavoritesCommand = new RelayCommand<StoryElement>(AddToFavorites);
        RemoveFromFavoritesCommand = new RelayCommand<StoryElement>(RemoveFromFavorites);

        // Initialize collections
        RecentItems = new ObservableCollection<StoryElement>();
        Favorites = new ObservableCollection<StoryElement>();
        AllElements = new ObservableCollection<StoryElement>();
        SearchResults = new ObservableCollection<StoryElement>();
    }

    #region Properties

    /// <summary>
    /// Primary context element being actively edited
    /// </summary>
    public StoryElement? PrimaryContext
    {
        get => _primaryContext;
        set
        {
            if (_primaryContext != value)
            {
                _primaryContext = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PrimaryContextType));
                OnPropertyChanged(nameof(PrimaryContextTitle));
                UpdateAIContext();
            }
        }
    }

    /// <summary>
    /// Secondary context element for reference
    /// </summary>
    public StoryElement? SecondaryContext
    {
        get => _secondaryContext;
        set
        {
            if (_secondaryContext != value)
            {
                _secondaryContext = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SecondaryContextType));
                OnPropertyChanged(nameof(SecondaryContextTitle));
            }
        }
    }

    /// <summary>
    /// Tertiary context element for additional reference
    /// </summary>
    public StoryElement? TertiaryContext
    {
        get => _tertiaryContext;
        set
        {
            if (_tertiaryContext != value)
            {
                _tertiaryContext = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TertiaryContextType));
                OnPropertyChanged(nameof(TertiaryContextTitle));
            }
        }
    }

    public string PrimaryContextType => PrimaryContext?.ElementType.ToString() ?? "None";
    public string PrimaryContextTitle => PrimaryContext?.Title ?? "No Primary Context";
    public string SecondaryContextType => SecondaryContext?.ElementType.ToString() ?? "None";
    public string SecondaryContextTitle => SecondaryContext?.Title ?? "No Secondary Context";
    public string TertiaryContextType => TertiaryContext?.ElementType.ToString() ?? "None";
    public string TertiaryContextTitle => TertiaryContext?.Title ?? "No Tertiary Context";

    /// <summary>
    /// Recent items for quick access
    /// </summary>
    public ObservableCollection<StoryElement> RecentItems { get; }

    /// <summary>
    /// Favorite items for quick access
    /// </summary>
    public ObservableCollection<StoryElement> Favorites { get; }

    /// <summary>
    /// All story elements for search and navigation
    /// </summary>
    public ObservableCollection<StoryElement> AllElements { get; }

    /// <summary>
    /// Search results
    /// </summary>
    public ObservableCollection<StoryElement> SearchResults { get; }

    /// <summary>
    /// Search text
    /// </summary>
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                PerformSearch();
            }
        }
    }

    /// <summary>
    /// Current AI context for enhanced AI interactions
    /// </summary>
    public string CurrentAIContext { get; private set; } = string.Empty;

    #endregion

    #region Commands

    public ICommand SetPrimaryContextCommand { get; }
    public ICommand SetSecondaryContextCommand { get; }
    public ICommand ClearContextCommand { get; }
    public ICommand CreateElementCommand { get; }
    public ICommand LinkElementsCommand { get; }
    public ICommand NavigateToElementCommand { get; }
    public ICommand AddToFavoritesCommand { get; }
    public ICommand RemoveFromFavoritesCommand { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Set primary context element
    /// </summary>
    private void SetPrimaryContext(StoryElement? element)
    {
        if (element == null) return;

        // Move current primary to secondary if exists
        if (PrimaryContext != null)
        {
            SecondaryContext = PrimaryContext;
        }

        PrimaryContext = element;
        AddToRecentItems(element);
    }

    /// <summary>
    /// Set secondary context element
    /// </summary>
    private void SetSecondaryContext(StoryElement? element)
    {
        if (element == null) return;
        SecondaryContext = element;
        AddToRecentItems(element);
    }

    /// <summary>
    /// Clear context element
    /// </summary>
    private void ClearContext(StoryElement? element)
    {
        if (element == PrimaryContext)
        {
            PrimaryContext = SecondaryContext;
            SecondaryContext = TertiaryContext;
            TertiaryContext = null;
        }
        else if (element == SecondaryContext)
        {
            SecondaryContext = TertiaryContext;
            TertiaryContext = null;
        }
        else if (element == TertiaryContext)
        {
            TertiaryContext = null;
        }
    }

    /// <summary>
    /// Create new story element
    /// </summary>
    private async void CreateElement(ElementType elementType)
    {
        try
        {
            switch (elementType)
            {
                case ElementType.Universe:
                    await CreateUniverse();
                    break;
                case ElementType.Story:
                    await CreateStory();
                    break;
                case ElementType.Character:
                    await CreateCharacter();
                    break;
                case ElementType.Chapter:
                    await CreateChapter();
                    break;
                case ElementType.Location:
                    await CreateLocation();
                    break;
            }
        }
        catch (Exception ex)
        {
            // Handle error
            System.Diagnostics.Debug.WriteLine($"Error creating element: {ex.Message}");
        }
    }

    /// <summary>
    /// Link elements together
    /// </summary>
    private async void LinkElements(object? parameter)
    {
        if (parameter is not (StoryElement element1, StoryElement element2)) return;

        try
        {
            // Implement element linking logic
            await LinkStoryElements(element1, element2);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error linking elements: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigate to element
    /// </summary>
    private void NavigateToElement(StoryElement? element)
    {
        if (element == null) return;
        SetPrimaryContext(element);
    }

    /// <summary>
    /// Add element to favorites
    /// </summary>
    private void AddToFavorites(StoryElement? element)
    {
        if (element == null || Favorites.Contains(element)) return;
        Favorites.Add(element);
    }

    /// <summary>
    /// Remove element from favorites
    /// </summary>
    private void RemoveFromFavorites(StoryElement? element)
    {
        if (element == null) return;
        Favorites.Remove(element);
    }

    /// <summary>
    /// Add element to recent items
    /// </summary>
    private void AddToRecentItems(StoryElement element)
    {
        // Remove if already exists
        RecentItems.Remove(element);
        
        // Add to beginning
        RecentItems.Insert(0, element);
        
        // Limit to 10 items
        while (RecentItems.Count > 10)
        {
            RecentItems.RemoveAt(RecentItems.Count - 1);
        }
    }

    /// <summary>
    /// Perform search
    /// </summary>
    private void PerformSearch()
    {
        SearchResults.Clear();
        
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return;
        }

        var searchTerm = SearchText.ToLower();
        var results = AllElements
            .Where(e => e.Title.ToLower().Contains(searchTerm) || 
                       e.Description.ToLower().Contains(searchTerm))
            .Take(20);

        foreach (var result in results)
        {
            SearchResults.Add(result);
        }
    }

    /// <summary>
    /// Update AI context based on current contexts
    /// </summary>
    private void UpdateAIContext()
    {
        var contextBuilder = new System.Text.StringBuilder();
        
        if (PrimaryContext != null)
        {
            contextBuilder.AppendLine($"Primary Context: {PrimaryContext.Title} ({PrimaryContext.ElementType})");
            contextBuilder.AppendLine($"Description: {PrimaryContext.Description}");
        }

        if (SecondaryContext != null)
        {
            contextBuilder.AppendLine($"Secondary Context: {SecondaryContext.Title} ({SecondaryContext.ElementType})");
            contextBuilder.AppendLine($"Description: {SecondaryContext.Description}");
        }

        if (TertiaryContext != null)
        {
            contextBuilder.AppendLine($"Tertiary Context: {TertiaryContext.Title} ({TertiaryContext.ElementType})");
            contextBuilder.AppendLine($"Description: {TertiaryContext.Description}");
        }

        CurrentAIContext = contextBuilder.ToString();
        OnPropertyChanged(nameof(CurrentAIContext));
    }

    #endregion

    #region Private Helper Methods

    private async Task CreateUniverse()
    {
        if (_dialogService.ShowInputDialog("Enter universe name:", "New Universe", out var name))
        {
            var universe = new Universe { Name = name };
            var created = await _universeRepository.AddAsync(universe);
            var element = new StoryElement(created);
            AllElements.Add(element);
            SetPrimaryContext(element);
        }
    }

    private async Task CreateStory()
    {
        if (PrimaryContext?.ElementType != ElementType.Universe)
        {
            // Show error or prompt to select universe
            return;
        }

        if (_dialogService.ShowInputDialog("Enter story name:", "New Story", out var name))
        {
            var story = new Story { Name = name, UniverseId = PrimaryContext.Id };
            var created = await _storyRepository.AddAsync(story);
            var element = new StoryElement(created);
            AllElements.Add(element);
            SetSecondaryContext(element);
        }
    }

    private async Task CreateCharacter()
    {
        if (_dialogService.ShowInputDialog("Enter character name:", "New Character", out var name))
        {
            var character = new Character { Name = name };
            var created = await _characterRepository.AddAsync(character);
            var element = new StoryElement(created);
            AllElements.Add(element);
            SetSecondaryContext(element);
        }
    }

    private async Task CreateChapter()
    {
        if (PrimaryContext?.ElementType != ElementType.Story)
        {
            // Show error or prompt to select story
            return;
        }

        if (_dialogService.ShowInputDialog("Enter chapter title:", "New Chapter", out var title))
        {
            var chapter = new Chapter { Title = title, StoryId = PrimaryContext.Id };
            var created = await _chapterRepository.AddAsync(chapter);
            var element = new StoryElement(created);
            AllElements.Add(element);
            SetSecondaryContext(element);
        }
    }

    private async Task CreateLocation()
    {
        if (_dialogService.ShowInputDialog("Enter location name:", "New Location", out var name))
        {
            // Create location element (assuming we have a Location model)
            var element = new StoryElement(ElementType.Location, name, "New location");
            AllElements.Add(element);
            SetSecondaryContext(element);
        }
    }

    private async Task LinkStoryElements(StoryElement element1, StoryElement element2)
    {
        // Implement linking logic based on element types
        // This would depend on the specific relationships you want to support
        await Task.CompletedTask;
    }

    #endregion
}

/// <summary>
/// Story element wrapper for unified handling
/// </summary>
public class StoryElement
{
    public ElementType ElementType { get; }
    public Guid Id { get; }
    public string Title { get; }
    public string Description { get; }
    public object? OriginalObject { get; }

    public StoryElement(Universe universe)
    {
        ElementType = ElementType.Universe;
        Id = universe.Id.Value;
        Title = universe.Name.Value;
        Description = universe.Description?.Value ?? "Universe";
        OriginalObject = universe;
    }

    public StoryElement(Story story)
    {
        ElementType = ElementType.Story;
        Id = story.Id.Value;
        Title = story.Name.Value;
        Description = story.Description?.Value ?? "Story";
        OriginalObject = story;
    }

    public StoryElement(Character character)
    {
        ElementType = ElementType.Character;
        Id = character.Id.Value;
        Title = character.Name.Value;
        Description = character.Description?.Value ?? "Character";
        OriginalObject = character;
    }

    public StoryElement(Chapter chapter)
    {
        ElementType = ElementType.Chapter;
        Id = chapter.Id.Value;
        Title = chapter.Title.Value;
        Description = chapter.Content?.Value ?? "Chapter";
        OriginalObject = chapter;
    }

    public StoryElement(ElementType elementType, string title, string description)
    {
        ElementType = elementType;
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        OriginalObject = null;
    }
}

/// <summary>
/// Story element types
/// </summary>
public enum ElementType
{
    Universe,
    Story,
    Character,
    Chapter,
    Location
}
