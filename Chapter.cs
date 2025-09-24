namespace Genisis.Core.Models;

public class Chapter
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public int ChapterOrder { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int StoryId { get; set; }
    public Story? Story { get; set; }
    public ICollection<Character> Characters { get; set; } = new List<Character>();
}