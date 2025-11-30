using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Dashboard;

namespace TechHaven.Presentation.WinUI.Services.Mock
{
    public class MockDashboardService : IDashboardService
    {
        public Task<ResponseWrapper<DashboardDto>> GetDashboardAsync()
        {
            var img = "ms-appx:///Assets/phone.jpg";
            var dashboard = new DashboardDto
            {
                TotalProducts = 120,
                TodayOrderCount = 5,
                TodayRevenue = 1250000m,
                LowStockProducts = new List<LowStockProductDto>
                {
                    new LowStockProductDto { ProductId = 14, ProductName = "Samsung Galaxy Z Fold5", BrandName = "Samsung", StockQuantity = 4, SellPrice = 4099000000, Image_Url = img },
                    new LowStockProductDto { ProductId = 5, ProductName = "iPhone 14 Pro", BrandName = "Apple", StockQuantity = 3, SellPrice = 26990000, Image_Url = img },
                    new LowStockProductDto { ProductId = 2, ProductName = "iPhone 15 Pro Max", BrandName = "Apple", StockQuantity = 2, SellPrice = 33990000, Image_Url = img },
                    new LowStockProductDto { ProductId = 31, ProductName = "Xiaomi 14T", BrandName = "Xiaomi", StockQuantity = 4, SellPrice = 14990000, Image_Url = img },
                    new LowStockProductDto { ProductId = 45, ProductName = "POCO F6 Pro", BrandName = "Xiaomi", StockQuantity = 1, SellPrice = 15990000, Image_Url = img }
                },
                TopSellingProducts = new List<TopSellingProductDto>
                {
                    new TopSellingProductDto { ProductId = 1, ProductName = "iPhone 15 Pro", BrandName = "Apple", TotalSold = 150, TotalRevenue = 150 * 289900000m, Image_Url = img },
                    new TopSellingProductDto { ProductId = 12, ProductName = "Samsung Galaxy S24+", BrandName = "Samsung", TotalSold = 120, TotalRevenue = 120 * 23990000m, Image_Url = img },
                    new TopSellingProductDto { ProductId = 6, ProductName = "iPhone 13", BrandName = "Apple", TotalSold = 95, TotalRevenue = 95 * 17990000m, Image_Url = img },
                    new TopSellingProductDto { ProductId = 21, ProductName = "Xiaomi 14T", BrandName = "Xiaomi", TotalSold = 80, TotalRevenue = 80 * 14990000m, Image_Url = img },
                    new TopSellingProductDto { ProductId = 19, ProductName = "Samsung Galaxy A15", BrandName = "Samsung", TotalSold = 70, TotalRevenue = 70 * 4990000m, Image_Url = img }
                },
                RecentOrders = new List<RecentOrderDto>
                {
                    new RecentOrderDto { OrderId = 1001, CustomerName = "Nguyễn Văn A", OrderDate = DateTime.Now.AddHours(-1), TotalAmount = 12990000m, Status = "Completed" },
                    new RecentOrderDto { OrderId = 1002, CustomerName = "Trần Thị B", OrderDate = DateTime.Now.AddHours(-3), TotalAmount = 4590000m, Status = "Processing" },
                    new RecentOrderDto { OrderId = 1003, CustomerName = "Lê Văn C", OrderDate = DateTime.Now.AddDays(-1), TotalAmount = 2390000m, Status = "Cancelled" }
                },
                // Provide exactly 30 points: last 30 days (ending today)
                MonthlyRevenue = Enumerable.Range(0, 30)
                    .Select(i =>
                    {
                        var date = DateTime.Now.Date.AddDays(i - 29); // last 30 days
                        // deterministic-ish random per date
                        var seed = date.Day + date.Month * 31 + date.Year;
                        var rnd = new Random(seed);
                        return new DailyRevenueDto
                        {
                            Date = date,
                            Revenue = (decimal)rnd.Next(0, 2000000),
                            OrderCount = rnd.Next(0, 10)
                        };
                    }).ToList()
             };

             var response = new ResponseWrapper<DashboardDto>
             {
                 Success = true,
                 Message = "Dashboard retrieved successfully",
                 Data = dashboard
             };

             return Task.FromResult(response);
         }
     }
 }
