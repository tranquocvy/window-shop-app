using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechHaven.Domain.SearchCriteria
{
    public class ProductSearchCriteria : GenericSearchCriteria
    {
        public bool? IsDraft { get; set; }

        //Lọc theo khoảng giá
        public int? FromPrice { get; set; }
        public int? ToPrice { get; set; }

        //Lọc theo hãng
        public string? Brand { get; set; }

        //Lọc theo trạng thái
        public ProductStatus? Status { get; set; }
    }

    /// <summary>
    /// Defines the types of customers in the system.
    /// </summary>
    public enum ProductStatus
    {
        InStock = 1, // Còn hàng
        OutOfStock = 2, // Hết hàng
    }
}