namespace Genisis.Core.Models;

public class Universe
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Story> Stories { get; set; } = new List<Story>();
    public ICollection<Character> Characters { get; set; } = new List<Character>();
}