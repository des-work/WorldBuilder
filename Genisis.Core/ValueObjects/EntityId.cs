using System.ComponentModel.DataAnnotations;

namespace Genisis.Core.ValueObjects;

/// <summary>
/// Base class for strongly-typed entity identifiers
/// </summary>
/// <typeparam name="T">The type of the identifier value</typeparam>
public abstract class EntityId<T> : IEquatable<EntityId<T>> where T : notnull
{
    protected EntityId(T value)
    {
        Value = value;
    }

    public T Value { get; }

    public bool Equals(EntityId<T>? other)
    {
        return other is not null && Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        return obj is EntityId<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value.ToString() ?? string.Empty;
    }

    public static bool operator ==(EntityId<T>? left, EntityId<T>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(EntityId<T>? left, EntityId<T>? right)
    {
        return !Equals(left, right);
    }
}

/// <summary>
/// Strongly-typed identifier for Universe entities
/// </summary>
public class UniverseId : EntityId<int>
{
    public UniverseId(int value) : base(value)
    {
        if (value <= 0)
            throw new ArgumentException("Universe ID must be positive", nameof(value));
    }

    public static implicit operator int(UniverseId id) => id.Value;
    public static implicit operator UniverseId(int value) => new(value);
}

/// <summary>
/// Strongly-typed identifier for Story entities
/// </summary>
public class StoryId : EntityId<int>
{
    public StoryId(int value) : base(value)
    {
        if (value <= 0)
            throw new ArgumentException("Story ID must be positive", nameof(value));
    }

    public static implicit operator int(StoryId id) => id.Value;
    public static implicit operator StoryId(int value) => new(value);
}

/// <summary>
/// Strongly-typed identifier for Character entities
/// </summary>
public class CharacterId : EntityId<int>
{
    public CharacterId(int value) : base(value)
    {
        if (value <= 0)
            throw new ArgumentException("Character ID must be positive", nameof(value));
    }

    public static implicit operator int(CharacterId id) => id.Value;
    public static implicit operator CharacterId(int value) => new(value);
}

/// <summary>
/// Strongly-typed identifier for Chapter entities
/// </summary>
public class ChapterId : EntityId<int>
{
    public ChapterId(int value) : base(value)
    {
        if (value <= 0)
            throw new ArgumentException("Chapter ID must be positive", nameof(value));
    }

    public static implicit operator int(ChapterId id) => id.Value;
    public static implicit operator ChapterId(int value) => new(value);
}
