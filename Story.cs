namespace Genisis.Core.Models;

public class Story
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Logline { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UniverseId { get; set; }
    public Universe? Universe { get; set; }
    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
}