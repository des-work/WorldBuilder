using System.ComponentModel.DataAnnotations;

namespace Genisis.Core.ValueObjects;

/// <summary>
/// Base class for text-based value objects with validation
/// </summary>
public abstract class TextValue : IEquatable<TextValue>
{
    protected TextValue(string value, int maxLength, bool allowEmpty = false)
    {
        if (string.IsNullOrWhiteSpace(value) && !allowEmpty)
            throw new ArgumentException("Value cannot be null or whitespace", nameof(value));
        
        if (value.Length > maxLength)
            throw new ArgumentException($"Value cannot exceed {maxLength} characters", nameof(value));

        Value = value.Trim();
    }

    public string Value { get; }

    public bool Equals(TextValue? other)
    {
        return other is not null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is TextValue other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(TextValue? left, TextValue? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TextValue? left, TextValue? right)
    {
        return !Equals(left, right);
    }
}

/// <summary>
/// Value object for entity names with validation
/// </summary>
public class EntityName : TextValue
{
    public EntityName(string value) : base(value, 200)
    {
    }

    public static implicit operator string(EntityName name) => name.Value;
    public static implicit operator EntityName(string value) => new(value);
}

/// <summary>
/// Value object for entity descriptions with validation
/// </summary>
public class EntityDescription : TextValue
{
    public EntityDescription(string value) : base(value, 2000, allowEmpty: true)
    {
    }

    public static implicit operator string(EntityDescription description) => description.Value;
    public static implicit operator EntityDescription(string value) => new(value);
}

/// <summary>
/// Value object for story loglines with validation
/// </summary>
public class StoryLogline : TextValue
{
    public StoryLogline(string value) : base(value, 500, allowEmpty: true)
    {
    }

    public static implicit operator string(StoryLogline logline) => logline.Value;
    public static implicit operator StoryLogline(string value) => new(value);
}

/// <summary>
/// Value object for character biographies with validation
/// </summary>
public class CharacterBio : TextValue
{
    public CharacterBio(string value) : base(value, 5000, allowEmpty: true)
    {
    }

    public static implicit operator string(CharacterBio bio) => bio.Value;
    public static implicit operator CharacterBio(string value) => new(value);
}

/// <summary>
/// Value object for character notes with validation
/// </summary>
public class CharacterNotes : TextValue
{
    public CharacterNotes(string value) : base(value, 2000, allowEmpty: true)
    {
    }

    public static implicit operator string(CharacterNotes notes) => notes.Value;
    public static implicit operator CharacterNotes(string value) => new(value);
}

/// <summary>
/// Value object for chapter content with validation
/// </summary>
public class ChapterContent : TextValue
{
    public ChapterContent(string value) : base(value, 10000, allowEmpty: true)
    {
    }

    public static implicit operator string(ChapterContent content) => content.Value;
    public static implicit operator ChapterContent(string value) => new(value);
}
