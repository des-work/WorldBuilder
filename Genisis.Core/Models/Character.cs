using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Genisis.Core.Models;

public enum CharacterTier
{
    Main,
    Recurring,
    Side
}

public class Character : INotifyPropertyChanged
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    private string _name = string.Empty;
    public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

    [Required]
    private CharacterTier _tier;
    public CharacterTier Tier { get => _tier; set { _tier = value; OnPropertyChanged(); } }

    [StringLength(5000)]
    private string? _bio;
    public string? Bio { get => _bio; set { _bio = value; OnPropertyChanged(); } }

    [StringLength(2000)]
    private string? _notes;
    public string? Notes { get => _notes; set { _notes = value; OnPropertyChanged(); } }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UniverseId { get; set; }
    public Universe? Universe { get; set; }
    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
