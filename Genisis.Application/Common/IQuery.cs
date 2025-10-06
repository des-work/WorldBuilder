namespace Genisis.Application.Common;

/// <summary>
/// Marker interface for queries
/// </summary>
public interface IQuery
{
}

/// <summary>
/// Query with a result
/// </summary>
/// <typeparam name="TResult">The type of result</typeparam>
public interface IQuery<out TResult> : IQuery
{
}

/// <summary>
/// Query handler interface
/// </summary>
/// <typeparam name="TQuery">The type of query</typeparam>
/// <typeparam name="TResult">The type of result</typeparam>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
