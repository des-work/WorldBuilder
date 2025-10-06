using Genisis.Core.Models;
using Genisis.Core.ValueObjects;
using Genisis.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Genisis.Tests.Infrastructure;

/// <summary>
/// Interface for test data builder
/// </summary>
public interface ITestDataBuilder
{
    Task<object> BuildDataSetAsync(string dataSetName);
    Task<Universe> CreateTestUniverseAsync();
    Task<Story> CreateTestStoryAsync(UniverseId universeId);
    Task<Character> CreateTestCharacterAsync(UniverseId universeId);
    Task<Chapter> CreateTestChapterAsync(StoryId storyId);
    Task<List<Universe>> CreateMultipleUniversesAsync(int count);
    Task<List<Story>> CreateMultipleStoriesAsync(UniverseId universeId, int count);
    Task<List<Character>> CreateMultipleCharactersAsync(UniverseId universeId, int count);
}

/// <summary>
/// Test data builder implementation
/// </summary>
public class TestDataBuilder : ITestDataBuilder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TestDataBuilder> _logger;
    private readonly Random _random = new();

    public TestDataBuilder(IServiceProvider serviceProvider, ILogger<TestDataBuilder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<object> BuildDataSetAsync(string dataSetName)
    {
        return dataSetName switch
        {
            "BasicUniverse" => await CreateTestUniverseAsync(),
            "BasicStory" => await CreateTestStoryAsync(UniverseId.New()),
            "BasicCharacter" => await CreateTestCharacterAsync(UniverseId.New()),
            "BasicChapter" => await CreateTestChapterAsync(StoryId.New()),
            "MultipleUniverses" => await CreateMultipleUniversesAsync(5),
            "MultipleStories" => await CreateMultipleStoriesAsync(UniverseId.New(), 3),
            "MultipleCharacters" => await CreateMultipleCharactersAsync(UniverseId.New(), 10),
            "FullStoryUniverse" => await CreateFullStoryUniverseAsync(),
            _ => throw new ArgumentException($"Unknown data set: {dataSetName}")
        };
    }

    public async Task<Universe> CreateTestUniverseAsync()
    {
        var universe = new Universe(
            UniverseId.New(),
            EntityName.Create("Test Universe"),
            TextValue.Create("A test universe for testing purposes")
        );

        // Save to database if service provider is available
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GenesisDbContext>();
            dbContext.Universes.Add(universe);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save test universe to database");
        }

        return universe;
    }

    public async Task<Story> CreateTestStoryAsync(UniverseId universeId)
    {
        var story = new Story(
            StoryId.New(),
            universeId,
            EntityName.Create("Test Story"),
            TextValue.Create("A test story for testing purposes"),
            TextValue.Create("A hero's journey through a magical realm")
        );

        // Save to database if service provider is available
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GenesisDbContext>();
            dbContext.Stories.Add(story);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save test story to database");
        }

        return story;
    }

    public async Task<Character> CreateTestCharacterAsync(UniverseId universeId)
    {
        var characterNames = new[]
        {
            "Aria", "Bren", "Cora", "Dax", "Elena", "Finn", "Gwen", "Hugo", "Iris", "Jace"
        };

        var characterDescriptions = new[]
        {
            "A brave warrior with a mysterious past",
            "A wise mage who seeks knowledge",
            "A cunning rogue with quick wit",
            "A noble knight sworn to protect",
            "A skilled archer with keen eyes",
            "A powerful sorcerer wielding ancient magic",
            "A stealthy assassin with deadly precision",
            "A charismatic bard who tells tales",
            "A fierce barbarian from distant lands",
            "A scholarly wizard studying the arcane"
        };

        var name = characterNames[_random.Next(characterNames.Length)];
        var description = characterDescriptions[_random.Next(characterDescriptions.Length)];

        var character = new Character(
            CharacterId.New(),
            universeId,
            EntityName.Create(name),
            TextValue.Create(description),
            CharacterTier.Main
        );

        // Save to database if service provider is available
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GenesisDbContext>();
            dbContext.Characters.Add(character);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save test character to database");
        }

        return character;
    }

    public async Task<Chapter> CreateTestChapterAsync(StoryId storyId)
    {
        var chapterTitles = new[]
        {
            "The Beginning", "A New Journey", "The Challenge", "The Discovery", "The Conflict",
            "The Resolution", "The Return", "The Transformation", "The Final Battle", "The Conclusion"
        };

        var chapterContents = new[]
        {
            "The story begins with our hero discovering their true destiny.",
            "A new adventure awaits as the journey takes an unexpected turn.",
            "Challenges arise that test the hero's resolve and determination.",
            "A crucial discovery changes everything the hero thought they knew.",
            "Conflict escalates as opposing forces clash in dramatic fashion.",
            "The resolution brings closure to the story's central conflict.",
            "The hero returns home, forever changed by their experiences.",
            "A transformation occurs that reshapes the hero's understanding.",
            "The final battle determines the fate of all involved.",
            "The conclusion ties together all the story's loose ends."
        };

        var title = chapterTitles[_random.Next(chapterTitles.Length)];
        var content = chapterContents[_random.Next(chapterContents.Length)];

        var chapter = new Chapter(
            ChapterId.New(),
            storyId,
            EntityName.Create(title),
            TextValue.Create(content),
            _random.Next(1, 20)
        );

        // Save to database if service provider is available
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GenesisDbContext>();
            dbContext.Chapters.Add(chapter);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save test chapter to database");
        }

        return chapter;
    }

    public async Task<List<Universe>> CreateMultipleUniversesAsync(int count)
    {
        var universes = new List<Universe>();
        
        for (int i = 0; i < count; i++)
        {
            var universe = new Universe(
                UniverseId.New(),
                EntityName.Create($"Test Universe {i + 1}"),
                TextValue.Create($"Test universe number {i + 1} for testing purposes")
            );
            
            universes.Add(universe);
        }

        // Save to database if service provider is available
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GenesisDbContext>();
            dbContext.Universes.AddRange(universes);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save test universes to database");
        }

        return universes;
    }

    public async Task<List<Story>> CreateMultipleStoriesAsync(UniverseId universeId, int count)
    {
        var stories = new List<Story>();
        
        for (int i = 0; i < count; i++)
        {
            var story = new Story(
                StoryId.New(),
                universeId,
                EntityName.Create($"Test Story {i + 1}"),
                TextValue.Create($"Test story number {i + 1} for testing purposes"),
                TextValue.Create($"A compelling narrative about adventure number {i + 1}")
            );
            
            stories.Add(story);
        }

        // Save to database if service provider is available
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GenesisDbContext>();
            dbContext.Stories.AddRange(stories);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save test stories to database");
        }

        return stories;
    }

    public async Task<List<Character>> CreateMultipleCharactersAsync(UniverseId universeId, int count)
    {
        var characters = new List<Character>();
        
        for (int i = 0; i < count; i++)
        {
            var character = await CreateTestCharacterAsync(universeId);
            characters.Add(character);
        }

        return characters;
    }

    private async Task<object> CreateFullStoryUniverseAsync()
    {
        // Create a complete story universe with all components
        var universe = await CreateTestUniverseAsync();
        var stories = await CreateMultipleStoriesAsync(universe.Id, 3);
        var characters = await CreateMultipleCharactersAsync(universe.Id, 5);
        
        var chapters = new List<Chapter>();
        foreach (var story in stories)
        {
            var storyChapters = new List<Chapter>();
            for (int i = 0; i < 3; i++)
            {
                var chapter = await CreateTestChapterAsync(story.Id);
                storyChapters.Add(chapter);
            }
            chapters.AddRange(storyChapters);
        }

        return new
        {
            Universe = universe,
            Stories = stories,
            Characters = characters,
            Chapters = chapters
        };
    }
}
