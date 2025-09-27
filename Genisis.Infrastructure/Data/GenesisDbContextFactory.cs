using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace Genisis.Infrastructure.Data;

public class GenesisDbContextFactory : IDesignTimeDbContextFactory<GenesisDbContext>
{
    public GenesisDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<GenesisDbContext>();
        builder.UseSqlite($"Data Source={Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WorldBuilderAI", "worldbuilder.db")}");
        return new GenesisDbContext(builder.Options);
    }
}
