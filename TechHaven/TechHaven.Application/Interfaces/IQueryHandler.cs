using MediatR;

namespace TechHaven.Application.Interfaces;

/// <summary>
/// Handler for queries that return a result.
/// Query handlers should never modify state.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle.</typeparam>
/// <typeparam name="TResponse">The type of result returned.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse> { }