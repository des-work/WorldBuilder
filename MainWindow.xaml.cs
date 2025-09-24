using Genisis.Core.Repositories;
using System.Windows;

namespace Genisis.App;

public partial class MainWindow : Window
{
    private readonly IUniverseRepository _universeRepository;

    public MainWindow(IUniverseRepository universeRepository)
    {
        InitializeComponent();
        _universeRepository = universeRepository; // The repository is now available to use
    }
}