using Genisis.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Genisis.Core.Data;

public class GenesisDbContext : DbContext
{
    public DbSet<Universe> Universes { get; set; }
    public DbSet<Story> Stories { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Chapter> Chapters { get; set; }

    public GenesisDbContext(DbContextOptions<GenesisDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the many-to-many relationship between Chapter and Character
        // EF Core will automatically create the 'chapter_characters' join table.
        modelBuilder.Entity<Chapter>()
            .HasMany(c => c.Characters)
            .WithMany(c => c.Chapters);

        // Configure the CharacterTier enum to be stored as a string
        modelBuilder.Entity<Character>()
            .Property(c => c.Tier)
            .HasConversion<string>();
    }
}