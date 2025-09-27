using Genisis.Core.Models;
using Genisis.Core.Repositories;
using Genisis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Genisis.Tests;

public class IntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly GenesisDbContext _dbContext;

    public IntegrationTests()
    {
        var services = new ServiceCollection();

        // Configure in-memory database for testing
        services.AddDbContext<GenesisDbContext>(options =>
            options.UseSqlite("DataSource=:memory:"));

        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Register repositories
        services.AddScoped<IUniverseRepository, Infrastructure.Repositories.UniverseRepository>();
        services.AddScoped<IStoryRepository, Infrastructure.Repositories.StoryRepository>();
        services.AddScoped<ICharacterRepository, Infrastructure.Repositories.CharacterRepository>();
        services.AddScoped<IChapterRepository, Infrastructure.Repositories.ChapterRepository>();

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<GenesisDbContext>();

        // Create the database and apply migrations
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task FullWorkflow_IntegrationTest()
    {
        // Arrange
        var universeRepo = _serviceProvider.GetRequiredService<IUniverseRepository>();
        var storyRepo = _serviceProvider.GetRequiredService<IStoryRepository>();
        var characterRepo = _serviceProvider.GetRequiredService<ICharacterRepository>();
        var chapterRepo = _serviceProvider.GetRequiredService<IChapterRepository>();

        // Act & Assert - Create Universe
        var universe = new Universe
        {
            Name = "Test Universe",
            Description = "A test universe for integration testing"
        };

        var createdUniverse = await universeRepo.AddAsync(universe);
        Assert.NotEqual(0, createdUniverse.Id);
        Assert.Equal("Test Universe", createdUniverse.Name);

        // Act & Assert - Create Characters
        var hero = new Character
        {
            Name = "Test Hero",
            Tier = CharacterTier.Main,
            Bio = "A brave hero on a quest",
            UniverseId = createdUniverse.Id
        };

        var villain = new Character
        {
            Name = "Test Villain",
            Tier = CharacterTier.Main,
            Bio = "An evil villain causing trouble",
            UniverseId = createdUniverse.Id
        };

        var createdHero = await characterRepo.AddAsync(hero);
        var createdVillain = await characterRepo.AddAsync(villain);

        Assert.NotEqual(0, createdHero.Id);
        Assert.NotEqual(0, createdVillain.Id);
        Assert.Equal(CharacterTier.Main, createdHero.Tier);

        // Act & Assert - Create Story
        var story = new Story
        {
            Name = "Test Story",
            Logline = "A test story for integration testing",
            UniverseId = createdUniverse.Id
        };

        var createdStory = await storyRepo.AddAsync(story);
        Assert.NotEqual(0, createdStory.Id);
        Assert.Equal("Test Story", createdStory.Name);

        // Act & Assert - Create Chapters
        var chapter1 = new Chapter
        {
            Title = "Chapter 1",
            ChapterOrder = 1,
            Content = "This is the first chapter content",
            StoryId = createdStory.Id
        };

        var chapter2 = new Chapter
        {
            Title = "Chapter 2",
            ChapterOrder = 2,
            Content = "This is the second chapter content",
            StoryId = createdStory.Id
        };

        var createdChapter1 = await chapterRepo.AddAsync(chapter1);
        var createdChapter2 = await chapterRepo.AddAsync(chapter2);

        Assert.NotEqual(0, createdChapter1.Id);
        Assert.NotEqual(0, createdChapter2.Id);
        Assert.Equal(1, createdChapter1.ChapterOrder);

        // Act & Assert - Link Characters to Chapters
        // Need to add chapters to characters and update the database
        await _dbContext.SaveChangesAsync(); // Ensure all entities are tracked

        createdChapter1.Characters.Add(createdHero);
        createdChapter2.Characters.Add(createdHero);
        createdChapter2.Characters.Add(createdVillain);

        await _dbContext.SaveChangesAsync();

        // Verify relationships
        var retrievedHero = await characterRepo.GetByIdAsync(createdHero.Id);
        var retrievedVillain = await characterRepo.GetByIdAsync(createdVillain.Id);

        Assert.NotNull(retrievedHero);
        Assert.NotNull(retrievedVillain);
        Assert.Equal(2, retrievedHero.Chapters.Count);
        Assert.Single(retrievedVillain.Chapters);

        // Act & Assert - Test Save Operations via Repository
        createdUniverse.Description = "Updated description";
        await universeRepo.UpdateAsync(createdUniverse);

        var updatedUniverse = await universeRepo.GetByIdAsync(createdUniverse.Id);
        Assert.NotNull(updatedUniverse);
        Assert.Equal("Updated description", updatedUniverse.Description);
    }

    [Fact]
    public async Task CharacterChapterLinking_IntegrationTest()
    {
        // Arrange
        var universeRepo = _serviceProvider.GetRequiredService<IUniverseRepository>();
        var storyRepo = _serviceProvider.GetRequiredService<IStoryRepository>();
        var characterRepo = _serviceProvider.GetRequiredService<ICharacterRepository>();
        var chapterRepo = _serviceProvider.GetRequiredService<IChapterRepository>();

        var universe = new Universe { Name = "Link Test Universe" };
        var createdUniverse = await universeRepo.AddAsync(universe);

        var story = new Story
        {
            Name = "Link Test Story",
            UniverseId = createdUniverse.Id
        };
        var createdStory = await storyRepo.AddAsync(story);

        var character = new Character
        {
            Name = "Link Test Character",
            UniverseId = createdUniverse.Id
        };
        var createdCharacter = await characterRepo.AddAsync(character);

        var chapter = new Chapter
        {
            Title = "Link Test Chapter",
            ChapterOrder = 1,
            Content = "Testing character-chapter linking",
            StoryId = createdStory.Id
        };
        var createdChapter = await chapterRepo.AddAsync(chapter);

        // Act - Link character to chapter
        createdCharacter.Chapters.Add(createdChapter);
        await characterRepo.UpdateAsync(createdCharacter);

        // Assert - Verify the relationship works
        var chaptersWithCharacter = await chapterRepo.GetByCharacterIdAsync(createdCharacter.Id);
        Assert.Single(chaptersWithCharacter);
        Assert.Equal(createdChapter.Id, chaptersWithCharacter.First().Id);
    }

    public void Dispose()
    {
        _dbContext.Database.CloseConnection();
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }
}
