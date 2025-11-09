using MediatR;

namespace TechHaven.Application.Common.Abstractions;

/// <summary>
/// Marker interface for commands that don't return a value.
/// </summary>
public interface ICommand : IRequest { }

/// <summary>
/// Interface for commands that return a result.
/// </summary>
/// <typeparam name="TResponse">The type of result returned by the command.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse> { }