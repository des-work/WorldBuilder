using Genisis.Core.ValueObjects;
using Xunit;

namespace Genisis.Tests.Unit.ValueObjects;

public class EntityIdTests
{
    [Fact]
    public void UniverseId_WithValidValue_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var id = new UniverseId(1);

        // Assert
        Assert.Equal(1, id.Value);
    }

    [Fact]
    public void UniverseId_WithZeroValue_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new UniverseId(0));
    }

    [Fact]
    public void UniverseId_WithNegativeValue_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new UniverseId(-1));
    }

    [Fact]
    public void UniverseId_ImplicitConversion_ShouldWork()
    {
        // Arrange
        var id = new UniverseId(42);

        // Act
        int intValue = id;
        UniverseId newId = 42;

        // Assert
        Assert.Equal(42, intValue);
        Assert.Equal(42, newId.Value);
    }

    [Fact]
    public void UniverseId_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var id1 = new UniverseId(1);
        var id2 = new UniverseId(1);
        var id3 = new UniverseId(2);

        // Act & Assert
        Assert.Equal(id1, id2);
        Assert.NotEqual(id1, id3);
        Assert.True(id1 == id2);
        Assert.False(id1 == id3);
        Assert.False(id1 != id2);
        Assert.True(id1 != id3);
    }

    [Fact]
    public void UniverseId_ToString_ShouldReturnValueAsString()
    {
        // Arrange
        var id = new UniverseId(123);

        // Act
        var result = id.ToString();

        // Assert
        Assert.Equal("123", result);
    }
}
