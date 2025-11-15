using System.Linq.Expressions;

namespace TechHaven.Domain.Interfaces
{
    /// <summary>
    /// Generic repository interface for basic CRUD operations.
    /// Simplified for small-scale projects (10 entities).
    /// </summary>
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Entry point for building queries (AsNoTracking by default).
        /// No SQL is executed until materialized (ToListAsync, CountAsync, etc.).
        /// </summary>
        IQueryable<TEntity> Query();

        // Read operations
        Task<TEntity?> GetByIdAsync(
            object id,
            CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<int> CountAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            CancellationToken cancellationToken = default);

        Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);


        // Convenience helper if needed (optional)
        Task<List<TEntity>> ToListAsync(
            IQueryable<TEntity> query,
            CancellationToken cancellationToken = default);

        // Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        // Task<IReadOnlyList<TEntity>> GetAsync(
        //     Expression<Func<TEntity, bool>>? predicate = null,
        //     Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        //     CancellationToken cancellationToken = default);

        // Write operations
        Task<TEntity> AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default);

        Task AddRangeAsync(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            TEntity entity,
            CancellationToken cancellationToken = default);

        Task RemoveAsync(
            TEntity entity,
            CancellationToken cancellationToken = default);

        Task RemoveRangeAsync(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default);

        // // Save changes - Thay thế Unit of Work
        // Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}