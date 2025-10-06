using Genisis.Application.Common;
using Genisis.Core.Models;
using Genisis.Core.ValueObjects;
using MediatR;

namespace Genisis.Application.Universe.Commands;

/// <summary>
/// Command to create a new universe
/// </summary>
public record CreateUniverseCommand(EntityName Name, EntityDescription? Description) : IRequest<Result<Universe>>;

/// <summary>
/// Handler for creating a universe
/// </summary>
public class CreateUniverseCommandHandler : IRequestHandler<CreateUniverseCommand, Result<Universe>>
{
    private readonly IUniverseRepository _universeRepository;
    private readonly IUniverseDomainService _universeDomainService;

    public CreateUniverseCommandHandler(IUniverseRepository universeRepository, IUniverseDomainService universeDomainService)
    {
        _universeRepository = universeRepository ?? throw new ArgumentNullException(nameof(universeRepository));
        _universeDomainService = universeDomainService ?? throw new ArgumentNullException(nameof(universeDomainService));
    }

    public async Task<Result<Universe>> Handle(CreateUniverseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate the creation
            var validationResult = await _universeDomainService.ValidateCreationAsync(request.Name, request.Description, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result<Universe>.Failure(validationResult.Errors);
            }

            // Check if name is unique
            var isNameUnique = await _universeDomainService.IsNameUniqueAsync(request.Name, null, cancellationToken);
            if (!isNameUnique)
            {
                return Result<Universe>.Failure($"A universe with the name '{request.Name.Value}' already exists.");
            }

            // Create the universe
            var universe = new Universe(request.Name, request.Description);
            var createdUniverse = await _universeRepository.AddAsync(universe, cancellationToken);

            return Result<Universe>.Success(createdUniverse);
        }
        catch (Exception ex)
        {
            return Result<Universe>.Failure($"Failed to create universe: {ex.Message}");
        }
    }
}
