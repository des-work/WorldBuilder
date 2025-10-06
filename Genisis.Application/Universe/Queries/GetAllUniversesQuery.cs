using Genisis.Application.Common;
using Genisis.Core.Models;
using MediatR;

namespace Genisis.Application.Universe.Queries;

/// <summary>
/// Query to get all universes
/// </summary>
public record GetAllUniversesQuery : IRequest<Result<List<Universe>>>;

/// <summary>
/// Handler for getting all universes
/// </summary>
public class GetAllUniversesQueryHandler : IRequestHandler<GetAllUniversesQuery, Result<List<Universe>>>
{
    private readonly IUniverseRepository _universeRepository;

    public GetAllUniversesQueryHandler(IUniverseRepository universeRepository)
    {
        _universeRepository = universeRepository ?? throw new ArgumentNullException(nameof(universeRepository));
    }

    public async Task<Result<List<Universe>>> Handle(GetAllUniversesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var universes = await _universeRepository.GetAllOrderedByNameAsync(cancellationToken);
            return Result<List<Universe>>.Success(universes);
        }
        catch (Exception ex)
        {
            return Result<List<Universe>>.Failure($"Failed to get universes: {ex.Message}");
        }
    }
}
