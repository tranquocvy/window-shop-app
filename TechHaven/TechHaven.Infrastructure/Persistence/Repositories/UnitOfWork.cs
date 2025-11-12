using Microsoft.EntityFrameworkCore.Storage;
using TechHaven.Domain.Interfaces;
using TechHaven.Infrastructure.Persistence.Repositories;

namespace TechHaven.Infrastructure.Persistence;

/// <summary>
/// Implementation of the Unit of Work pattern.
/// Coordinates multiple repositories and manages a single database transaction.
/// All repositories share the same DbContext instance, ensuring consistency.
/// </summary>
public class UnitOfWork : IUnitOfWork
{

}