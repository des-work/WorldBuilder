namespace Genisis.Core.Models;

public enum CharacterTier
{
    Main,
    Recurring,
    Side
}

public class Character
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public CharacterTier Tier { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UniverseId { get; set; }
    public Universe? Universe { get; set; }
    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
}