using Genisis.Core.Models;
using System.Collections.ObjectModel;

namespace Genisis.Presentation.Wpf.ViewModels;

/// <summary>
/// A view model used to represent the "Characters" folder in the TreeView.
/// This is a UI-only construct.
/// </summary>
public class CharacterFolderViewModel
{
    public string Name => "Characters";
    public Universe ParentUniverse { get; }
    public ObservableCollection<Character> Characters { get; } = new();

    public CharacterFolderViewModel(Universe parentUniverse)
    {
        ParentUniverse = parentUniverse;
    }
}
