using Genisis.Core.Models;
using Genisis.Core.ValueObjects;

namespace Genisis.Infrastructure.Specifications;

/// <summary>
/// Specification for finding universes by name
/// </summary>
public class UniverseByNameSpecification : Specification<Universe>
{
    public UniverseByNameSpecification(EntityName name)
    {
        Criteria = u => u.Name.Value == name.Value;
        IsTrackingEnabled = false;
    }
}

/// <summary>
/// Specification for finding universes by name excluding a specific ID
/// </summary>
public class UniverseByNameExcludingIdSpecification : Specification<Universe>
{
    public UniverseByNameExcludingIdSpecification(EntityName name, UniverseId excludeId)
    {
        Criteria = u => u.Name.Value == name.Value && u.Id != excludeId.Value;
        IsTrackingEnabled = false;
    }
}

/// <summary>
/// Specification for getting universe with stories and characters
/// </summary>
public class UniverseWithContentSpecification : Specification<Universe>
{
    public UniverseWithContentSpecification(UniverseId universeId)
    {
        Criteria = u => u.Id == universeId.Value;
        AddInclude(u => u.Stories);
        AddInclude(u => u.Characters);
        IsTrackingEnabled = false;
    }
}

/// <summary>
/// Specification for getting all universes ordered by name
/// </summary>
public class AllUniversesOrderedByNameSpecification : Specification<Universe>
{
    public AllUniversesOrderedByNameSpecification()
    {
        AddOrderBy(u => u.Name.Value);
        IsTrackingEnabled = false;
    }
}

/// <summary>
/// Specification for getting universes by search term
/// </summary>
public class UniverseSearchSpecification : Specification<Universe>
{
    public UniverseSearchSpecification(string searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            Criteria = u => u.Name.Value.ToLower().Contains(term) || 
                           (u.Description != null && u.Description.Value.ToLower().Contains(term));
        }
        AddOrderBy(u => u.Name.Value);
        IsTrackingEnabled = false;
    }
}
