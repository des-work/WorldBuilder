using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genisis.Core.Services;

/// <summary>
/// Interface for smart query system
/// </summary>
public interface ISmartQuerySystem
{
    /// <summary>
    /// Process a smart query
    /// </summary>
    /// <param name="query">Smart query to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query response</returns>
    Task<QueryResponse> ProcessQueryAsync(SmartQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a query plan
    /// </summary>
    /// <param name="query">Smart query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query plan</returns>
    Task<QueryPlan> CreateQueryPlanAsync(SmartQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimize a query
    /// </summary>
    /// <param name="query">Smart query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query optimization</returns>
    Task<QueryOptimization> OptimizeQueryAsync(SmartQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Build query context
    /// </summary>
    /// <param name="query">Smart query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query context</returns>
    Task<QueryContext> BuildContextAsync(SmartQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a query plan
    /// </summary>
    /// <param name="plan">Query plan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query result</returns>
    Task<QueryResult> ExecuteQueryAsync(QueryPlan plan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze query intent
    /// </summary>
    /// <param name="userInput">User input</param>
    /// <param name="context">Query context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query intent analysis</returns>
    Task<QueryIntentAnalysis> AnalyzeIntentAsync(string userInput, QueryContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get query suggestions
    /// </summary>
    /// <param name="context">Query context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query suggestions</returns>
    Task<IEnumerable<QuerySuggestion>> GetQuerySuggestionsAsync(QueryContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate query
    /// </summary>
    /// <param name="query">Smart query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query validation result</returns>
    Task<QueryValidationResult> ValidateQueryAsync(SmartQuery query, CancellationToken cancellationToken = default);
}
