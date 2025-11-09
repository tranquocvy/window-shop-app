using MediatR;

namespace TechHaven.Application.Common.Abstractions;

/// <summary>
/// Handler for commands that don't return a value.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
  where TCommand : ICommand { }

/// <summary>
/// Handler for commands that return a result.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
/// <typeparam name="TResponse">The type of result returned.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
  where TCommand : ICommand<TResponse> { }