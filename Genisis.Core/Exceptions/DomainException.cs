namespace Genisis.Core.Exceptions;

/// <summary>
/// Base exception for domain-related errors
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a domain validation fails
/// </summary>
public class DomainValidationException : DomainException
{
    public IReadOnlyList<string> ValidationErrors { get; }

    public DomainValidationException(IEnumerable<string> validationErrors) 
        : base($"Domain validation failed: {string.Join(", ", validationErrors)}")
    {
        ValidationErrors = validationErrors.ToList().AsReadOnly();
    }

    public DomainValidationException(string validationError) 
        : base($"Domain validation failed: {validationError}")
    {
        ValidationErrors = new List<string> { validationError }.AsReadOnly();
    }
}

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleViolationException(string ruleName, string message) 
        : base($"Business rule '{ruleName}' violated: {message}")
    {
        RuleName = ruleName;
    }
}

/// <summary>
/// Exception thrown when an entity is not found
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object entityId) 
        : base($"{entityType} with ID '{entityId}' was not found")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Exception thrown when an operation is not allowed
/// </summary>
public class OperationNotAllowedException : DomainException
{
    public string Operation { get; }
    public string Reason { get; }

    public OperationNotAllowedException(string operation, string reason) 
        : base($"Operation '{operation}' is not allowed: {reason}")
    {
        Operation = operation;
        Reason = reason;
    }
}
