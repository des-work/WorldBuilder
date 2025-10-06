using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Genisis.Presentation.Wpf.ViewModels;

namespace Genisis.Presentation.Wpf.Controls;

/// <summary>
/// Seamless navigation control with breadcrumbs, quick actions, and search
/// </summary>
public partial class SeamlessNavigationControl : UserControl, INotifyPropertyChanged
{
    private NonLinearStoryViewModel? _viewModel;
    private bool _isSearchVisible;
    private string _searchText = string.Empty;

    public SeamlessNavigationControl()
    {
        InitializeComponent();
        InitializeCommands();
    }

    #region Properties

    /// <summary>
    /// Parent view model
    /// </summary>
    public NonLinearStoryViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            if (_viewModel != value)
            {
                _viewModel = value;
                OnPropertyChanged();
                UpdateBindings();
            }
        }
    }

    /// <summary>
    /// Breadcrumb path for navigation
    /// </summary>
    public ObservableCollection<StoryElement> BreadcrumbPath { get; } = new();

    /// <summary>
    /// Recent items for quick access
    /// </summary>
    public ObservableCollection<StoryElement> RecentItems { get; } = new();

    /// <summary>
    /// Favorite items
    /// </summary>
    public ObservableCollection<StoryElement> Favorites { get; } = new();

    /// <summary>
    /// All story elements
    /// </summary>
    public ObservableCollection<StoryElement> AllElements { get; } = new();

    /// <summary>
    /// Search results
    /// </summary>
    public ObservableCollection<StoryElement> SearchResults { get; } = new();

    /// <summary>
    /// Search text
    /// </summary>
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
    /// Is search visible
    /// </summary>
    public bool IsSearchVisible
    {
        get => _isSearchVisible;
        set
        {
            if (_isSearchVisible != value)
            {
                _isSearchVisible = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Commands

    public ICommand ShowRecentCommand { get; private set; } = null!;
    public ICommand ShowFavoritesCommand { get; private set; } = null!;
    public ICommand ToggleSearchCommand { get; private set; } = null!;
    public ICommand ShowTimelineCommand { get; private set; } = null!;
    public ICommand ShowRelationshipsCommand { get; private set; } = null!;
    public ICommand NavigateToElementCommand { get; private set; } = null!;

    #endregion

    #region Methods

    /// <summary>
    /// Initialize commands
    /// </summary>
    private void InitializeCommands()
    {
        ShowRecentCommand = new RelayCommand(ShowRecent);
        ShowFavoritesCommand = new RelayCommand(ShowFavorites);
        ToggleSearchCommand = new RelayCommand(ToggleSearch);
        ShowTimelineCommand = new RelayCommand(ShowTimeline);
        ShowRelationshipsCommand = new RelayCommand(ShowRelationships);
        NavigateToElementCommand = new RelayCommand<StoryElement>(NavigateToElement);
    }

    /// <summary>
    /// Update bindings when view model changes
    /// </summary>
    private void UpdateBindings()
    {
        if (_viewModel == null) return;

        // Update collections from view model
        RecentItems.Clear();
        foreach (var item in _viewModel.RecentItems)
        {
            RecentItems.Add(item);
        }

        Favorites.Clear();
        foreach (var item in _viewModel.Favorites)
        {
            Favorites.Add(item);
        }

        AllElements.Clear();
        foreach (var item in _viewModel.AllElements)
        {
            AllElements.Add(item);
        }

        // Update breadcrumb path
        UpdateBreadcrumbPath();
    }

    /// <summary>
    /// Update breadcrumb path based on current context
    /// </summary>
    private void UpdateBreadcrumbPath()
    {
        BreadcrumbPath.Clear();

        if (_viewModel?.PrimaryContext == null) return;

        var currentElement = _viewModel.PrimaryContext;
        var path = new List<StoryElement>();

        // Build path based on element hierarchy
        switch (currentElement.ElementType)
        {
            case ElementType.Chapter:
                // Find parent story
                var story = FindParentStory(currentElement);
                if (story != null)
                {
                    path.Add(story);
                    // Find parent universe
                    var universe = FindParentUniverse(story);
                    if (universe != null)
                    {
                        path.Insert(0, universe);
                    }
                }
                path.Add(currentElement);
                break;

            case ElementType.Story:
                // Find parent universe
                var parentUniverse = FindParentUniverse(currentElement);
                if (parentUniverse != null)
                {
                    path.Add(parentUniverse);
                }
                path.Add(currentElement);
                break;

            case ElementType.Character:
                // Characters can belong to multiple stories
                path.Add(currentElement);
                break;

            case ElementType.Universe:
                path.Add(currentElement);
                break;

            case ElementType.Location:
                path.Add(currentElement);
                break;
        }

        // Add to breadcrumb path
        foreach (var element in path)
        {
            BreadcrumbPath.Add(element);
        }
    }

    /// <summary>
    /// Show recent items
    /// </summary>
    private void ShowRecent()
    {
        // Focus on recent items section
        // This would expand the recent items expander
    }

    /// <summary>
    /// Show favorites
    /// </summary>
    private void ShowFavorites()
    {
        // Focus on favorites section
        // This would expand the favorites expander
    }

    /// <summary>
    /// Toggle search visibility
    /// </summary>
    private void ToggleSearch()
    {
        IsSearchVisible = !IsSearchVisible;
        
        if (IsSearchVisible)
        {
            // Focus search box
            SearchBox?.Focus();
        }
    }

    /// <summary>
    /// Show timeline view
    /// </summary>
    private void ShowTimeline()
    {
        // Navigate to timeline view
        // This would trigger a timeline navigation event
    }

    /// <summary>
    /// Show relationships view
    /// </summary>
    private void ShowRelationships()
    {
        // Navigate to relationships view
        // This would trigger a relationships navigation event
    }

    /// <summary>
    /// Navigate to element
    /// </summary>
    private void NavigateToElement(StoryElement? element)
    {
        if (element == null || _viewModel == null) return;

        // Set as primary context
        _viewModel.SetPrimaryContextCommand.Execute(element);
        
        // Update breadcrumb path
        UpdateBreadcrumbPath();
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

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Find parent story for an element
    /// </summary>
    private StoryElement? FindParentStory(StoryElement element)
    {
        // This would implement logic to find the parent story
        // For now, return null
        return null;
    }

    /// <summary>
    /// Find parent universe for an element
    /// </summary>
    private StoryElement? FindParentUniverse(StoryElement element)
    {
        // This would implement logic to find the parent universe
        // For now, return null
        return null;
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}

/// <summary>
/// Simple relay command implementation
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();
}

/// <summary>
/// Generic relay command implementation
/// </summary>
public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;

    public void Execute(object? parameter) => _execute((T?)parameter);
}
