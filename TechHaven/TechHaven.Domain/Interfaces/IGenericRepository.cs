using System.Linq.Expressions;
using TechHaven.Domain.Specifications;

namespace TechHaven.Domain.Interfaces
{
    /// <summary>
    /// Generic repository interface for basic CRUD operations.
    /// Simplified for small-scale projects (10 entities).
    /// </summary>
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        // Read operations
        Task<TEntity?>
        GetByIdAsync(
            object id,
            CancellationToken cancellationToken = default
        );

        Task<IReadOnlyList<TEntity>>
        GetAllAsync(
            CancellationToken cancellationToken = default
        );

        // OLD
        // Task<IReadOnlyList<TEntity>>
        // GetAsync(
        //     Expression<Func<TEntity, bool>>? predicate = null,
        //     Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        //     CancellationToken cancellationToken = default
        // );

        // NEW: APLLY SPECIFICATION PATTERN
        Task<IReadOnlyList<TEntity>>
        GetAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default
        );

        Task<TEntity?>
        FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default
        );

        Task<bool>
        AnyAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default
        );

        // OLD
        // Task<int>
        // CountAsync(
        //     Expression<Func<TEntity, bool>>? predicate = null,
        //     CancellationToken cancellationToken = default
        // );

        // NEW: APLLY SPECIFICATION PATTERN
        Task<int>
        CountAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default
        );

        // Write operations
        Task<TEntity>
        AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default
        );

        Task
        AddRangeAsync(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default
        );

        Task
        UpdateAsync(
            TEntity entity,
            CancellationToken cancellationToken = default
        );

        Task
        DeleteAsync(
            TEntity entity,
            CancellationToken cancellationToken = default
        );

        Task
        DeleteRangeAsync(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default
        );
    }
}