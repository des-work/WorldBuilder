using Genisis.Application.Common;
using Genisis.Core.Models;
using Genisis.Core.ValueObjects;
using MediatR;

namespace Genisis.Application.Universe.Queries;

/// <summary>
/// Query to get a universe by ID
/// </summary>
public record GetUniverseQuery(UniverseId Id) : IRequest<Result<Universe>>;

/// <summary>
/// Handler for getting a universe
/// </summary>
public class GetUniverseQueryHandler : IRequestHandler<GetUniverseQuery, Result<Universe>>
{
    private readonly IUniverseRepository _universeRepository;

    public GetUniverseQueryHandler(IUniverseRepository universeRepository)
    {
        _universeRepository = universeRepository ?? throw new ArgumentNullException(nameof(universeRepository));
    }

    public async Task<Result<Universe>> Handle(GetUniverseQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var universe = await _universeRepository.GetByIdAsync(request.Id.Value, cancellationToken);
            if (universe == null)
            {
                return Result<Universe>.Failure($"Universe with ID {request.Id.Value} not found.");
            }

            return Result<Universe>.Success(universe);
        }
        catch (Exception ex)
        {
            return Result<Universe>.Failure($"Failed to get universe: {ex.Message}");
        }
    }
}
