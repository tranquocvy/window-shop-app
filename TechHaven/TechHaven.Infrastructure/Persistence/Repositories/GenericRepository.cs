using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.Specifications;
using TechHaven.Infrastructure.Specifications;

namespace TechHaven.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository implementation providing basic CRUD operations for all entities.
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly ILogger _logger;
    private readonly string _entityName = typeof(TEntity).Name;

    public GenericRepository(
        AppDbContext context,
         ILoggerFactory loggerFactory
        )
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
        _logger = loggerFactory.CreateLogger($"Repository[{_entityName}]");
    }

    #region Read Operations

    public virtual async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetById",
            async () => await _dbSet.FindAsync(new[] { id }, cancellationToken),
            new { Id = id });
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    // public virtual async Task<IReadOnlyList<TEntity>> GetAsync(
    //     Expression<Func<TEntity, bool>>? predicate = null,
    //     Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
    //     CancellationToken cancellationToken = default)
    // {
    //     IQueryable<TEntity> query = _dbSet;

    //     if (predicate != null)
    //     {
    //         query = query.Where(predicate);
    //     }

    //     if (orderBy != null)
    //     {
    //         query = orderBy(query);
    //     }

    //     return await query.ToListAsync(cancellationToken);
    // }

    public async Task<IReadOnlyList<TEntity>> GetAsync(
       ISpecification<TEntity> specification,
       CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetWithSpecification",
            () =>
            {
                var query = SpecificationEvaluator<TEntity>.GetQuery(_dbSet, specification);
                return query.AsNoTracking().ToListAsync(cancellationToken);
            },
            new { Specification = specification.GetType().Name });
    }


    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "FirstOrDefault",
            () => _dbSet.FirstOrDefaultAsync(predicate, cancellationToken),
            new { Predicate = predicate.ToString() });
    }

    public virtual async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "Any",
            () => _dbSet.AnyAsync(predicate, cancellationToken),
            new { Predicate = predicate.ToString() });
    }

    // public virtual async Task<int> CountAsync(
    //     Expression<Func<TEntity, bool>>? predicate = null,
    //     CancellationToken cancellationToken = default)
    // {
    //     if (predicate == null)
    //     {
    //         return await _dbSet.CountAsync(cancellationToken);
    //     }

    //     return await _dbSet.CountAsync(predicate, cancellationToken);
    // }

    public async Task<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "CountWithSpecification",
            () =>
            {
                var query = SpecificationEvaluator<TEntity>.GetQuery(_dbSet, specification);
                return query.CountAsync(cancellationToken);
            },
            new { Specification = specification.GetType().Name });
    }

    #endregion

    #region Write Operations

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await ExecuteOperationAsync(
            "Add",
            async () => await _dbSet.AddAsync(entity, cancellationToken),
            new { Entity = _entityName });

        return entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await ExecuteOperationAsync(
            "AddRange",
            async () => await _dbSet.AddRangeAsync(entities, cancellationToken),
            new { Count = entities.Count() });
    }

    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return ExecuteOperationAsync(
            "Update",
            () =>
            {
                _dbSet.Update(entity);
                return Task.CompletedTask;
            },
            new { Entity = _entityName });
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return ExecuteOperationAsync(
            "Delete",
            () =>
            {
                _dbSet.Remove(entity);
                return Task.CompletedTask;
            },
            new { Entity = _entityName });
    }

    public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        return ExecuteOperationAsync(
            "DeleteRange",
            () =>
            {
                _dbSet.RemoveRange(entities);
                return Task.CompletedTask;
            },
            new { Count = entities.Count() });
    }

    #endregion

    protected virtual async Task<TResult> ExecuteOperationAsync<TResult>(
        string operationName,
        Func<Task<TResult>> action,
        object? metadata = null)
    {
        _logger.LogDebug(
            "Starting {Operation} on {Entity}. Context: {@Context}",
            operationName,
            _entityName,
            metadata);

        try
        {
            var result = await action();

            _logger.LogDebug(
                "Completed {Operation} on {Entity}",
                operationName,
                _entityName);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Database {Operation} failed for {Entity}. Context: {@Context}",
                operationName,
                _entityName,
                metadata);

            throw;
        }
    }

    protected Task ExecuteOperationAsync(
        string operationName,
        Func<Task> action,
        object? metadata = null)
    {
        return ExecuteOperationAsync(
            operationName,
            async () =>
            {
                await action();
                return true;
            },
            metadata);
    }
}