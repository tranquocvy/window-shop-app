using System;
using System.Collections.Generic;
using System.Threading;
using TechHaven.Domain.Entities;
using TechHaven.Domain.SearchCriteria;

namespace TechHaven.Domain.Interfaces
{

    /// <summary>
    /// Order-specific repository supporting IQueryable for
    /// server-side filtering, sorting, and paging.
    /// </summary>
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<(IReadOnlyList<Order> Items, int TotalCount)>
        SearchWithPaginationAsync( //Đã sửa ở đây để trùng với tên hàm và đúng ngữ cảnh hơn trong Repository
            OrderSearchCriteria criteria,
            CancellationToken cancellationToken = default
        );

        // Get detailed order info.
        Task<Order?> GetWithDetailsAsync(int orderId, CancellationToken cancellationToken = default);

        }
    }