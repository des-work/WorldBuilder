using Genisis.Application.Common;
using Genisis.Core.Models;
using MediatR;

namespace Genisis.Application.Universe.Queries;

/// <summary>
/// Query to search universes by term
/// </summary>
public record SearchUniversesQuery(string SearchTerm) : IRequest<Result<List<Universe>>>;

/// <summary>
/// Handler for searching universes
/// </summary>
public class SearchUniversesQueryHandler : IRequestHandler<SearchUniversesQuery, Result<List<Universe>>>
{
    private readonly IUniverseRepository _universeRepository;

    public SearchUniversesQueryHandler(IUniverseRepository universeRepository)
    {
        _universeRepository = universeRepository ?? throw new ArgumentNullException(nameof(universeRepository));
    }

    public async Task<Result<List<Universe>>> Handle(SearchUniversesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var universes = await _universeRepository.SearchAsync(request.SearchTerm, cancellationToken);
            return Result<List<Universe>>.Success(universes);
        }
        catch (Exception ex)
        {
            return Result<List<Universe>>.Failure($"Failed to search universes: {ex.Message}");
        }
    }
}
