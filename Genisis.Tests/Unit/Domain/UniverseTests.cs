using Genisis.Core.Models;
using Genisis.Core.ValueObjects;
using Xunit;

namespace Genisis.Tests.Unit.Domain;

public class UniverseTests
{
    [Fact]
    public void Universe_WithValidName_ShouldCreateSuccessfully()
    {
        // Arrange
        var name = new EntityName("Test Universe");
        var description = new EntityDescription("A test universe");

        // Act
        var universe = new Universe(name, description);

        // Assert
        Assert.Equal(name, universe.Name);
        Assert.Equal(description, universe.Description);
        Assert.NotNull(universe.DomainEvents);
        Assert.Single(universe.DomainEvents);
        Assert.IsType<UniverseCreatedEvent>(universe.DomainEvents.First());
    }

    [Fact]
    public void Universe_WithNullName_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Universe(null!, null));
    }

    [Fact]
    public void Universe_Update_ShouldUpdatePropertiesAndRaiseEvent()
    {
        // Arrange
        var universe = new Universe(new EntityName("Original Name"), new EntityDescription("Original Description"));
        var newName = new EntityName("New Name");
        var newDescription = new EntityDescription("New Description");

        // Act
        universe.Update(newName, newDescription);

        // Assert
        Assert.Equal(newName, universe.Name);
        Assert.Equal(newDescription, universe.Description);
        Assert.Equal(2, universe.DomainEvents.Count);
        Assert.IsType<UniverseUpdatedEvent>(universe.DomainEvents.Last());
    }

    [Fact]
    public void Universe_Update_WithNullName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var universe = new Universe(new EntityName("Test"), null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => universe.Update(null!, null));
    }

    [Fact]
    public void Universe_Delete_ShouldRaiseEvent()
    {
        // Arrange
        var universe = new Universe(new EntityName("Test"), null);

        // Act
        universe.Delete();

        // Assert
        Assert.Equal(2, universe.DomainEvents.Count);
        Assert.IsType<UniverseDeletedEvent>(universe.DomainEvents.Last());
    }

    [Fact]
    public void Universe_AddStory_ShouldAddStoryToCollection()
    {
        // Arrange
        var universe = new Universe(new EntityName("Test"), null);
        var storyName = new EntityName("Test Story");
        var logline = new StoryLogline("A test story");

        // Act
        var story = universe.AddStory(storyName, logline);

        // Assert
        Assert.Single(universe.Stories);
        Assert.Equal(story, universe.Stories.First());
        Assert.Equal(storyName, story.Name);
        Assert.Equal(logline, story.Logline);
    }

    [Fact]
    public void Universe_AddCharacter_ShouldAddCharacterToCollection()
    {
        // Arrange
        var universe = new Universe(new EntityName("Test"), null);
        var characterName = new EntityName("Test Character");
        var bio = new CharacterBio("A test character");

        // Act
        var character = universe.AddCharacter(characterName, CharacterTier.Main, bio);

        // Assert
        Assert.Single(universe.Characters);
        Assert.Equal(character, universe.Characters.First());
        Assert.Equal(characterName, character.Name);
        Assert.Equal(CharacterTier.Main, character.Tier);
        Assert.Equal(bio, character.Bio);
    }

    [Fact]
    public void Universe_ClearDomainEvents_ShouldClearAllEvents()
    {
        // Arrange
        var universe = new Universe(new EntityName("Test"), null);
        universe.Update(new EntityName("Updated"), null);
        universe.Delete();

        // Act
        universe.ClearDomainEvents();

        // Assert
        Assert.Empty(universe.DomainEvents);
    }
}
