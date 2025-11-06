using System.Collections.Generic;
using System.Threading;
using TechHaven.Domain.Entities;

namespace TechHaven.Domain.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category?> GetByNameAsync(string categoryName, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Category>> GetWithProductsAsync(CancellationToken cancellationToken = default);
    }
}