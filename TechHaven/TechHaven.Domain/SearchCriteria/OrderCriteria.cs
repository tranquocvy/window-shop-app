using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Domain.Enums;

namespace TechHaven.Domain.SearchCriteria
{
    public class OrderSearchCriteria : GenericSearchCriteria
    {
        //Specific attributes for searching Orders entity
        public int? CustomerId { get; set; } // Lọc theo khách hàng
        public int? UserId { get; set; } // Lọc theo nhân viên bán hàng
        public OrderStatus? Status { get; set; } // Lọc theo trạng thái đơn hàng (vd: pending, completed, cancelled)

        public decimal? MinTotalAmount { get; set; }
        public decimal? MaxTotalAmount { get; set; }

        public DateTime? FromDate { get; set; } //  ngày bắt đầu
        public DateTime? ToDate { get; set; } // ngày kết thúc
    }
}
