using Genisis.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genisis.Infrastructure.Data;

public class DataSeeder
{
    private readonly GenesisDbContext _dbContext;

    public DataSeeder(GenesisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Check if data already exists to prevent re-seeding
        if (await _dbContext.Universes.AnyAsync(cancellationToken))
        {
            return; // Database has already been seeded
        }

        var sampleUniverse = new Universe
        {
            Name = "The Aethelgard Chronicles",
            Description = "A universe of high fantasy, ancient magic, and forgotten empires.",
            Stories = new List<Story>
            {
                new() {
                    Name = "The Shadow of the Sunstone",
                    Logline = "A young cartographer discovers a relic that could either save her kingdom or plunge it into eternal darkness."
                }
            }
        };

        _dbContext.Universes.Add(sampleUniverse);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
