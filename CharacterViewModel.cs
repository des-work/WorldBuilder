using Genisis.Core.Models;
using Genisis.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Genisis.App.ViewModels;

public class CharacterViewModel : ViewModelBase
{
    private readonly ICharacterRepository _characterRepository;
    public Character Character { get; }

    public ICommand SaveCommand { get; }

    // Expose enum values for the ComboBox in the view
    public IEnumerable<CharacterTier> Tiers => Enum.GetValues(typeof(CharacterTier)).Cast<CharacterTier>();

    public CharacterViewModel(Character character, ICharacterRepository characterRepository)
    {
        Character = character;
        _characterRepository = characterRepository;
        SaveCommand = new RelayCommand(async _ => await SaveAsync());
    }

    private async Task SaveAsync()
    {
        await _characterRepository.UpdateAsync(Character);
    }
}