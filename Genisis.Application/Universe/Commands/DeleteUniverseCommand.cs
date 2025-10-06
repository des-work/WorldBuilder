using Genisis.Application.Common;
using Genisis.Core.Models;
using Genisis.Core.ValueObjects;
using MediatR;

namespace Genisis.Application.Universe.Commands;

/// <summary>
/// Command to delete a universe
/// </summary>
public record DeleteUniverseCommand(UniverseId Id) : IRequest<Result>;

/// <summary>
/// Handler for deleting a universe
/// </summary>
public class DeleteUniverseCommandHandler : IRequestHandler<DeleteUniverseCommand, Result>
{
    private readonly IUniverseRepository _universeRepository;
    private readonly IUniverseDomainService _universeDomainService;

    public DeleteUniverseCommandHandler(IUniverseRepository universeRepository, IUniverseDomainService universeDomainService)
    {
        _universeRepository = universeRepository ?? throw new ArgumentNullException(nameof(universeRepository));
        _universeDomainService = universeDomainService ?? throw new ArgumentNullException(nameof(universeDomainService));
    }

    public async Task<Result> Handle(DeleteUniverseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the existing universe
            var universe = await _universeRepository.GetByIdAsync(request.Id.Value, cancellationToken);
            if (universe == null)
            {
                return Result.Failure($"Universe with ID {request.Id.Value} not found.");
            }

            // Check if universe can be deleted
            var canDelete = await _universeDomainService.CanDeleteAsync(universe, cancellationToken);
            if (!canDelete)
            {
                return Result.Failure("Cannot delete universe that contains stories or characters.");
            }

            // Delete the universe
            universe.Delete();
            await _universeRepository.DeleteAsync(universe, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete universe: {ex.Message}");
        }
    }
}
