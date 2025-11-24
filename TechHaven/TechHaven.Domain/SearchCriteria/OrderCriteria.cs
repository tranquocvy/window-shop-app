using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechHaven.Domain.SearchCriteria
{
    public class OrderSearchCriteria : GenericSearchCriteria
    {
        //Specific attributes for searching Orders entity
        public int? CustomerId { get; set; } // Lọc theo khách hàng
        public int? UserId { get; set; } // Lọc theo nhân viên bán hàng
        public OrderStatus? Status { get; set; } // Lọc theo trạng thái đơn hàng (vd: pending, completed, cancelled)
        public DateTime? FromDate { get; set; } //  ngày bắt đầu
        public DateTime? ToDate { get; set; } // ngày kết thúc
    }

    //TODO: Xem xét di chuyển enum này vào Folder chung để cần tái sử dụng ở nhiều nơi
    public enum OrderStatus
    {
        /// <summary>
        /// Order is pending and awaiting processing.
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Order is currently being processed.
        /// </summary>
        Processing = 2,

        /// <summary>
        /// Order has been completed successfully.
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Order has been cancelled.
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// Order has been returned.
        /// </summary>
        Returned = 5
    }
}
