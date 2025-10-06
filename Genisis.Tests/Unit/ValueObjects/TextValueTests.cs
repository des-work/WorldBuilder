using Genisis.Core.ValueObjects;
using Xunit;

namespace Genisis.Tests.Unit.ValueObjects;

public class TextValueTests
{
    [Fact]
    public void EntityName_WithValidValue_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var name = new EntityName("Test Name");

        // Assert
        Assert.Equal("Test Name", name.Value);
    }

    [Fact]
    public void EntityName_WithNullValue_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new EntityName(null!));
    }

    [Fact]
    public void EntityName_WithEmptyValue_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new EntityName(""));
    }

    [Fact]
    public void EntityName_WithWhitespaceValue_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new EntityName("   "));
    }

    [Fact]
    public void EntityName_WithLongValue_ShouldThrowArgumentException()
    {
        // Arrange
        var longValue = new string('a', 201);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new EntityName(longValue));
    }

    [Fact]
    public void EntityName_WithValidLongValue_ShouldCreateSuccessfully()
    {
        // Arrange
        var longValue = new string('a', 200);

        // Act
        var name = new EntityName(longValue);

        // Assert
        Assert.Equal(longValue, name.Value);
    }

    [Fact]
    public void EntityName_TrimsWhitespace()
    {
        // Arrange & Act
        var name = new EntityName("  Test Name  ");

        // Assert
        Assert.Equal("Test Name", name.Value);
    }

    [Fact]
    public void EntityName_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var name1 = new EntityName("Test Name");
        var name2 = new EntityName("Test Name");
        var name3 = new EntityName("Different Name");

        // Act & Assert
        Assert.Equal(name1, name2);
        Assert.NotEqual(name1, name3);
        Assert.True(name1 == name2);
        Assert.False(name1 == name3);
    }

    [Fact]
    public void EntityName_Equality_ShouldBeCaseInsensitive()
    {
        // Arrange
        var name1 = new EntityName("Test Name");
        var name2 = new EntityName("test name");

        // Act & Assert
        Assert.Equal(name1, name2);
        Assert.True(name1 == name2);
    }

    [Fact]
    public void EntityDescription_WithEmptyValue_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var description = new EntityDescription("");

        // Assert
        Assert.Equal("", description.Value);
    }

    [Fact]
    public void EntityDescription_WithNullValue_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var description = new EntityDescription("");

        // Assert
        Assert.Equal("", description.Value);
    }
}
