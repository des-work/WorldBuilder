using Genisis.Core.Models;
using Genisis.Core.ValueObjects;

namespace Genisis.Infrastructure.Specifications;

/// <summary>
/// Specification for finding stories by universe ID
/// </summary>
public class StoriesByUniverseIdSpecification : Specification<Story>
{
    public StoriesByUniverseIdSpecification(UniverseId universeId)
    {
        Criteria = s => s.UniverseId.Value == universeId.Value;
        AddOrderBy(s => s.Name.Value);
        IsTrackingEnabled = false;
    }
}

/// <summary>
/// Specification for finding stories by name within a universe
/// </summary>
public class StoryByNameInUniverseSpecification : Specification<Story>
{
    public StoryByNameInUniverseSpecification(EntityName name, UniverseId universeId)
    {
        Criteria = s => s.Name.Value == name.Value && s.UniverseId.Value == universeId.Value;
        IsTrackingEnabled = false;
    }
}

/// <summary>
/// Specification for finding stories by name within a universe excluding a specific ID
/// </summary>
public class StoryByNameInUniverseExcludingIdSpecification : Specification<Story>
{
    public StoryByNameInUniverseExcludingIdSpecification(EntityName name, UniverseId universeId, StoryId excludeId)
    {
        Criteria = s => s.Name.Value == name.Value && 
                       s.UniverseId.Value == universeId.Value && 
                       s.Id != excludeId.Value;
        IsTrackingEnabled = false;
    }
}

/// <summary>
/// Specification for getting story with chapters
/// </summary>
public class StoryWithChaptersSpecification : Specification<Story>
{
    public StoryWithChaptersSpecification(StoryId storyId)
    {
        Criteria = s => s.Id == storyId.Value;
        AddInclude(s => s.Chapters);
        AddOrderBy(s => s.Chapters.Select(c => c.ChapterOrder));
        IsTrackingEnabled = false;
    }
}
