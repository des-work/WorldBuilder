using Genisis.Application.Common;
using Genisis.Core.Models;
using Genisis.Core.ValueObjects;
using MediatR;

namespace Genisis.Application.Universe.Commands;

/// <summary>
/// Command to update an existing universe
/// </summary>
public record UpdateUniverseCommand(UniverseId Id, EntityName Name, EntityDescription? Description) : IRequest<Result<Universe>>;

/// <summary>
/// Handler for updating a universe
/// </summary>
public class UpdateUniverseCommandHandler : IRequestHandler<UpdateUniverseCommand, Result<Universe>>
{
    private readonly IUniverseRepository _universeRepository;
    private readonly IUniverseDomainService _universeDomainService;

    public UpdateUniverseCommandHandler(IUniverseRepository universeRepository, IUniverseDomainService universeDomainService)
    {
        _universeRepository = universeRepository ?? throw new ArgumentNullException(nameof(universeRepository));
        _universeDomainService = universeDomainService ?? throw new ArgumentNullException(nameof(universeDomainService));
    }

    public async Task<Result<Universe>> Handle(UpdateUniverseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the existing universe
            var universe = await _universeRepository.GetByIdAsync(request.Id.Value, cancellationToken);
            if (universe == null)
            {
                return Result<Universe>.Failure($"Universe with ID {request.Id.Value} not found.");
            }

            // Validate the update
            var validationResult = await _universeDomainService.ValidateUpdateAsync(universe, request.Name, request.Description, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result<Universe>.Failure(validationResult.Errors);
            }

            // Check if name is unique (excluding current universe)
            var isNameUnique = await _universeDomainService.IsNameUniqueAsync(request.Name, request.Id, cancellationToken);
            if (!isNameUnique)
            {
                return Result<Universe>.Failure($"A universe with the name '{request.Name.Value}' already exists.");
            }

            // Update the universe
            universe.Update(request.Name, request.Description);
            await _universeRepository.UpdateAsync(universe, cancellationToken);

            return Result<Universe>.Success(universe);
        }
        catch (Exception ex)
        {
            return Result<Universe>.Failure($"Failed to update universe: {ex.Message}");
        }
    }
}
