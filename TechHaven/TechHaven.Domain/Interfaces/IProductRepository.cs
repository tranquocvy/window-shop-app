using System.Collections.Generic;
using System.Threading;
using TechHaven.Domain.Entities;

namespace TechHaven.Domain.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        // Tìm kiếm sản phẩm theo tên, trạng thái
        Task<IReadOnlyList<Product>> SearchAsync(
            string? searchTerm = null,
            bool? isDraft = null,
            CancellationToken cancellationToken = default);

        // Lấy sản phẩm tồn kho thấp hơn ngưỡng (bao gồm hết hàng nếu threshold = 0)
        Task<IReadOnlyList<Product>> GetLowStockAsync(int threshold = 0, CancellationToken cancellationToken = default);
    }
}