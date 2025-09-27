using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Genisis.Core.Models;

public class Chapter : INotifyPropertyChanged
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    private string _title = string.Empty;
    public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

    [Range(0, int.MaxValue)]
    public int ChapterOrder { get; set; }

    [StringLength(10000)]
    private string? _content;
    public string? Content { get => _content; set { _content = value; OnPropertyChanged(); } }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int StoryId { get; set; }
    public Story? Story { get; set; }
    public ICollection<Character> Characters { get; set; } = new List<Character>();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
