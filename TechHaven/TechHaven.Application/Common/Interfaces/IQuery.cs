using MediatR;

namespace TechHaven.Application.Common.Abstractions;

/// <summary>
/// Interface for queries that return a result.
/// Queries should never modify state.
/// </summary>
/// <typeparam name="TResponse">The type of result returned by the query.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse> { }