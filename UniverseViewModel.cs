﻿using Genisis.Core.Models;
using Genisis.Core.Repositories;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Genisis.App.ViewModels;

public class UniverseViewModel : ViewModelBase
{
    private readonly IUniverseRepository _universeRepository;
    public Universe Universe { get; }

    public ICommand SaveCommand { get; }

    public UniverseViewModel(Universe universe, IUniverseRepository universeRepository)
    {
        Universe = universe;
        _universeRepository = universeRepository;
        SaveCommand = new RelayCommand(async _ => await SaveAsync());
    }

    private async Task SaveAsync()
    {
        await _universeRepository.UpdateAsync(Universe);
    }
}