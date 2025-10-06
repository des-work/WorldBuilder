using Genisis.Core.Models;
using Genisis.Core.ValueObjects;

namespace Genisis.Infrastructure.Specifications;

/// <summary>
/// Specification for finding chapters by story ID
/// </summary>
public class ChaptersByStoryIdSpecification : Specification<Chapter>
{
    public ChaptersByStoryIdSpecification(StoryId storyId)
    {
        Criteria = c => c.StoryId.Value == storyId.Value;
        AddOrderBy(c => c.ChapterOrder);
        IsTrackingEnabled = false;
    }
}

/// <summary>
/// Specification for finding chapters by title within a story
/// </summary>
public class ChapterByTitleInStorySpecification : Specification<Chapter>
{
    public ChapterByTitleInStorySpecification(EntityName title, StoryId storyId)
    {
        Criteria = c => c.Title.Value == title.Value && c.StoryId.Value == storyId.Value;
        IsTrackingEnabled = false;
    }
}

/// <summary>
/// Specification for finding chapters by title within a story excluding a specific ID
/// </summary>
public class ChapterByTitleInStoryExcludingIdSpecification : Specification<Chapter>
{
    public ChapterByTitleInStoryExcludingIdSpecification(EntityName title, StoryId storyId, ChapterId excludeId)
    {
        Criteria = c => c.Title.Value == title.Value && 
                       c.StoryId.Value == storyId.Value && 
                       c.Id != excludeId.Value;
        IsTrackingEnabled = false;
    }
}

/// <summary>
/// Specification for getting chapter with characters
/// </summary>
public class ChapterWithCharactersSpecification : Specification<Chapter>
{
    public ChapterWithCharactersSpecification(ChapterId chapterId)
    {
        Criteria = c => c.Id == chapterId.Value;
        AddInclude(c => c.Characters);
        IsTrackingEnabled = false;
    }
}

/// <summary>
/// Specification for finding chapters by character ID
/// </summary>
public class ChaptersByCharacterIdSpecification : Specification<Chapter>
{
    public ChaptersByCharacterIdSpecification(CharacterId characterId)
    {
        Criteria = c => c.Characters.Any(ch => ch.Id == characterId.Value);
        AddOrderBy(c => c.ChapterOrder);
        IsTrackingEnabled = false;
    }
}
