using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Genisis.Core.Models;

public class Chapter : INotifyPropertyChanged
{
    public int Id { get; set; }

    private string _title = string.Empty;
    public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

    public int ChapterOrder { get; set; }

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
