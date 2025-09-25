﻿using Genisis.Core.Models;
using Genisis.Core.Repositories;
using System.Threading.Tasks;

namespace Genisis.App.ViewModels;

public class UniverseViewModel : EditorViewModelBase<Universe>
{
    private readonly IUniverseRepository _universeRepository;
    public Universe Universe => Model;

    public UniverseViewModel(Universe universe, IUniverseRepository universeRepository) : base(universe)
    {
        _universeRepository = universeRepository;
    }

    protected override async Task OnSaveAsync()
    {
        await _universeRepository.UpdateAsync(Universe);
    }
}