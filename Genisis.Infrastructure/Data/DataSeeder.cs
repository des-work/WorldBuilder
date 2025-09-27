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

    private static bool _seeded = false;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Only seed once per application lifetime
        if (_seeded) return;

        // Check if data already exists to prevent re-seeding
        if (await _dbContext.Universes.AnyAsync(cancellationToken))
        {
            _seeded = true;
            return; // Database has already been seeded
        }

        var fantasyUniverse = new Universe
        {
            Name = "The Aethelgard Chronicles",
            Description = "A universe of high fantasy, ancient magic, and forgotten empires where dragons soar and wizards weave spells that can reshape reality itself."
        };

        _dbContext.Universes.Add(fantasyUniverse);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Create main characters
        var hero = new Character
        {
            Name = "Elara Stormweaver",
            Tier = CharacterTier.Main,
            Bio = "A gifted young wizard with the rare ability to control storms and lightning. Her journey begins when she discovers an ancient artifact in the ruins of her village.",
            Notes = "Determined and compassionate, but struggles with the weight of her growing powers.",
            UniverseId = fantasyUniverse.Id
        };

        var mentor = new Character
        {
            Name = "Archmage Eldric",
            Tier = CharacterTier.Recurring,
            Bio = "An ancient wizard who has lived for centuries, guardian of forbidden knowledge and mentor to many heroes.",
            Notes = "Wise but secretive, holds many secrets about the ancient wars.",
            UniverseId = fantasyUniverse.Id
        };

        var antagonist = new Character
        {
            Name = "Lord Vesper Darkmoor",
            Tier = CharacterTier.Main,
            Bio = "A fallen noble who discovered dark magic in his quest for immortality, now seeks to conquer all of Aethelgard.",
            Notes = "Charismatic and intelligent, but consumed by ambition and fear of death.",
            UniverseId = fantasyUniverse.Id
        };

        await _dbContext.Characters.AddRangeAsync(hero, mentor, antagonist);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Create the main story
        var mainStory = new Story
        {
            Name = "The Shadow of the Sunstone",
            Logline = "When an ancient artifact awakens, a young wizard must master her powers to prevent a catastrophe that could engulf her world in eternal darkness.",
            UniverseId = fantasyUniverse.Id
        };

        _dbContext.Stories.Add(mainStory);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Create chapters
        var chapter1 = new Chapter
        {
            Title = "The Storm's Awakening",
            ChapterOrder = 1,
            Content = "The storm began as a whisper in the ancient forest. Elara felt the first tingle of unfamiliar magic in her fingertips as thunder rumbled in the distance. Little did she know this was just the beginning of a prophecy that would change her world forever.",
            StoryId = mainStory.Id
        };

        var chapter2 = new Chapter
        {
            Title = "The Ancient Mentor",
            ChapterOrder = 2,
            Content = "Archmage Eldric appeared from the mists, his eyes twinkling with ancient wisdom. 'The storms are awakening, child,' he said. 'You must learn to control your power before it consumes you.' Elara nodded, sensing the weight of destiny upon her shoulders.",
            StoryId = mainStory.Id
        };

        var chapter3 = new Chapter
        {
            Title = "Shadows in the Storm",
            ChapterOrder = 3,
            Content = "As Elara's powers grew, so did the shadows. Lord Vesper watched from his dark tower, his magic weaving through the storm clouds. The prophecy spoke of a choice between light and darkness, and Elara would soon face her greatest test.",
            StoryId = mainStory.Id
        };

        await _dbContext.Chapters.AddRangeAsync(chapter1, chapter2, chapter3);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Link characters to chapters
        hero.Chapters.Add(chapter1);
        hero.Chapters.Add(chapter2);
        hero.Chapters.Add(chapter3);
        mentor.Chapters.Add(chapter2);
        antagonist.Chapters.Add(chapter3);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _seeded = true;
    }
}
