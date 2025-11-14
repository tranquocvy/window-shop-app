using TechHaven.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace TechHaven.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        #region Read Operations
        public async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            //Anything to check ? - cần new[] {id} không ?
            return await _dbSet.FindAsync(new[] { id }, cancellationToken);
        }

        //Todo:
        //-> Cần cơ chế phân trang (pagination) để tránh trả về tất cả dữ liệu cùng lúc -> Nghẽn máy chủ
        //(ví dụ: GetPagedAsync(int pageNumber, int pageSize)).
        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }


        //Todo: Cần bổ sung cơ chế phân trang -> nếu như predicate khớp với quá nhiều bản ghi
        //Hàm này cần được mở rộng để nhận thêm tham số int skip và int take (hoặc pageNumber/pageSize)
        //  để áp dụng .Skip(skip).Take(take) trước khi gọi ToListAsync
        public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>>? predicate = null, 
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, 
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if(orderBy != null)
            {
                query = orderBy(query);
            }
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T,bool>> predicate, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        public Task<int> CountAsync(Expression<Func<T, 
            bool>>? predicate = null, 
            CancellationToken cancellationToken = default)
        {
            if (predicate == null)
           {
                return _dbSet.CountAsync(cancellationToken);
           }
           else
           {
                return _dbSet.CountAsync(predicate, cancellationToken);
            }
        }      
        #endregion



        #region Write Operations
        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(T entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}