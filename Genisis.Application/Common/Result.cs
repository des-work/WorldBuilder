namespace Genisis.Application.Common;

/// <summary>
/// Represents the result of an operation
/// </summary>
/// <typeparam name="T">The type of the result value</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }
    public List<string> Errors { get; }

    private Result(bool isSuccess, T? value, string? error, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        Errors = errors ?? new List<string>();
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>
    /// Creates a failed result with a single error
    /// </summary>
    public static Result<T> Failure(string error) => new(false, default, error, new List<string> { error });

    /// <summary>
    /// Creates a failed result with multiple errors
    /// </summary>
    public static Result<T> Failure(IEnumerable<string> errors) => new(false, default, null, errors.ToList());

    /// <summary>
    /// Creates a failed result with multiple errors
    /// </summary>
    public static Result<T> Failure(params string[] errors) => new(false, default, null, errors.ToList());

    /// <summary>
    /// Implicit conversion from value to successful result
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicit conversion from error to failed result
    /// </summary>
    public static implicit operator Result<T>(string error) => Failure(error);
}

/// <summary>
/// Represents the result of an operation without a value
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public List<string> Errors { get; }

    private Result(bool isSuccess, string? error, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors ?? new List<string>();
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a failed result with a single error
    /// </summary>
    public static Result Failure(string error) => new(false, error, new List<string> { error });

    /// <summary>
    /// Creates a failed result with multiple errors
    /// </summary>
    public static Result Failure(IEnumerable<string> errors) => new(false, null, errors.ToList());

    /// <summary>
    /// Creates a failed result with multiple errors
    /// </summary>
    public static Result Failure(params string[] errors) => new(false, null, errors.ToList());

    /// <summary>
    /// Implicit conversion from error to failed result
    /// </summary>
    public static implicit operator Result(string error) => Failure(error);
}
