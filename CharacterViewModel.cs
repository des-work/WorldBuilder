using Genisis.Core.Models;
using Genisis.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genisis.App.ViewModels;

public class CharacterViewModel : EditorViewModelBase<Character>
{
    private readonly ICharacterRepository _characterRepository;
    public Character Character => Model;

    // Expose enum values for the ComboBox in the view
    public IEnumerable<CharacterTier> Tiers => Enum.GetValues(typeof(CharacterTier)).Cast<CharacterTier>();

    public CharacterViewModel(Character character, ICharacterRepository characterRepository) : base(character)
    {
        _characterRepository = characterRepository;
    }

    protected override async Task OnSaveAsync()
    {
        await _characterRepository.UpdateAsync(Character);
    }
}