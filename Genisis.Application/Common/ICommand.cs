namespace Genisis.Application.Common;

/// <summary>
/// Marker interface for commands
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Command with a result
/// </summary>
/// <typeparam name="TResult">The type of result</typeparam>
public interface ICommand<out TResult> : ICommand
{
}

/// <summary>
/// Command handler interface
/// </summary>
/// <typeparam name="TCommand">The type of command</typeparam>
public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Command handler interface with result
/// </summary>
/// <typeparam name="TCommand">The type of command</typeparam>
/// <typeparam name="TResult">The type of result</typeparam>
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
