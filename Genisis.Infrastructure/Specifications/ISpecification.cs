using System.Linq.Expressions;

namespace Genisis.Infrastructure.Specifications;

/// <summary>
/// Specification pattern for building complex queries
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// The criteria expression for filtering
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// The expressions for including related entities
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// The expressions for ordering
    /// </summary>
    List<Expression<Func<T, object>>> OrderBy { get; }

    /// <summary>
    /// The expressions for ordering in descending order
    /// </summary>
    List<Expression<Func<T, object>>> OrderByDescending { get; }

    /// <summary>
    /// The number of items to take
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// The number of items to skip
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Whether to track changes
    /// </summary>
    bool IsTrackingEnabled { get; }
}

/// <summary>
/// Base implementation of specification pattern
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<Expression<Func<T, object>>> OrderBy { get; } = new();
    public List<Expression<Func<T, object>>> OrderByDescending { get; } = new();
    public int? Take { get; protected set; }
    public int? Skip { get; protected set; }
    public bool IsTrackingEnabled { get; protected set; } = true;

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy.Add(orderByExpression);
    }

    protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending.Add(orderByDescendingExpression);
    }
}
