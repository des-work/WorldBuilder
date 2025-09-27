using Genisis.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Genisis.Infrastructure.Data;

public class GenesisDbContext : DbContext
{
    public DbSet<Universe> Universes { get; set; }
    public DbSet<Story> Stories { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Chapter> Chapters { get; set; }

    public GenesisDbContext(DbContextOptions<GenesisDbContext> options) : base(options) { }

    // Design-time factory for EF Core tools
    public static GenesisDbContext CreateForDesignTime()
    {
        var builder = new DbContextOptionsBuilder<GenesisDbContext>();
        builder.UseSqlite($"Data Source={Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WorldBuilderAI", "worldbuilder.db")}");
        return new GenesisDbContext(builder.Options);
    }

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
