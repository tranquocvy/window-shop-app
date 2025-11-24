using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechHaven.Domain.SearchCriteria
{
    public class GenericSearchCriteria
    {
        public string? SearchTerm { get; set; } // Tìm theo mã đơn hàng, tên sản phẩm, v.v.

        //Cho phân trang & sắp xếp
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = true; // Mặc định đơn mới nhất lên đầu
    }
}
