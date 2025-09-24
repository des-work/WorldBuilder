using Genisis.Core.Models;

namespace Genisis.App.ViewModels;

public class UniverseViewModel : ViewModelBase
{
    public Universe Universe { get; }

    public UniverseViewModel(Universe universe)
    {
        Universe = universe;
    }
}