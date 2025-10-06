using Genisis.Core.Models;
using Genisis.Core.Repositories;
using Genisis.Core.Services;
using Genisis.Core.ValueObjects;

namespace Genisis.Infrastructure.Services;

/// <summary>
/// Implementation of universe domain service
/// </summary>
public class UniverseDomainService : IUniverseDomainService
{
    private readonly IUniverseRepository _universeRepository;

    public UniverseDomainService(IUniverseRepository universeRepository)
    {
        _universeRepository = universeRepository ?? throw new ArgumentNullException(nameof(universeRepository));
    }

    public async Task<bool> IsNameUniqueAsync(EntityName name, UniverseId? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _universeRepository.IsNameUniqueAsync(name, excludeId, cancellationToken);
    }

    public async Task<bool> CanDeleteAsync(Universe universe, CancellationToken cancellationToken = default)
    {
        // Get the content count to check if universe has stories or characters
        var contentCount = await GetContentCountAsync(universe, cancellationToken);
        return contentCount.StoryCount == 0 && contentCount.CharacterCount == 0;
    }

    public async Task<(int StoryCount, int CharacterCount)> GetContentCountAsync(Universe universe, CancellationToken cancellationToken = default)
    {
        // This would typically be optimized with a single query
        var stories = await _universeRepository.GetByUniverseIdAsync(new UniverseId(universe.Id), cancellationToken);
        var characters = await _universeRepository.GetByUniverseIdAsync(new UniverseId(universe.Id), cancellationToken);
        
        return (stories.Count, characters.Count);
    }

    public async Task<ValidationResult> ValidateCreationAsync(EntityName name, EntityDescription? description, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        // Basic validation
        if (name == null || string.IsNullOrWhiteSpace(name.Value))
        {
            errors.Add("Universe name is required.");
        }

        if (name != null && name.Value.Length > 200)
        {
            errors.Add("Universe name cannot exceed 200 characters.");
        }

        if (description != null && description.Value.Length > 2000)
        {
            errors.Add("Universe description cannot exceed 2000 characters.");
        }

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateUpdateAsync(Universe universe, EntityName name, EntityDescription? description, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        // Basic validation
        if (name == null || string.IsNullOrWhiteSpace(name.Value))
        {
            errors.Add("Universe name is required.");
        }

        if (name != null && name.Value.Length > 200)
        {
            errors.Add("Universe name cannot exceed 200 characters.");
        }

        if (description != null && description.Value.Length > 2000)
        {
            errors.Add("Universe description cannot exceed 2000 characters.");
        }

        // Check if universe exists
        if (universe == null)
        {
            errors.Add("Universe not found.");
        }

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
}
