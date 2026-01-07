using Bogus;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Enums;

namespace TechHaven.UnitTests.Common;
[ExcludeFromCodeCoverage] //Đây chỉ là File gen dữ liệu - không cần test

public static class DataGenerator
{
    // 1. Faker cơ bản cho các Entity độc lập (Product, Customer, User)
    public static Faker<Product> ProductFaker { get; } = new Faker<Product>()
        .RuleFor(p => p.ProductId, f => f.IndexFaker + 1)
        .RuleFor(p => p.ProductName, f => f.Commerce.ProductName())
        .RuleFor(p => p.BrandName, f => f.Company.CompanyName())
        .RuleFor(p => p.SellPrice, f => decimal.Parse(f.Commerce.Price(5000, 10000))*1000) // Đơn vị là VNĐ
        .RuleFor(p => p.CostPrice, (f, p) => p.SellPrice * 0.7m) // Cost thấp hơn Sell
        .RuleFor(p => p.StockQuantity, f => f.Random.Int(10, 100))
        .RuleFor(p => p.IsDraft, false);

    public static Faker<Customer> CustomerFaker { get; } = new Faker<Customer>()
        .RuleFor(c => c.CustomerId, f => f.IndexFaker + 1)
        .RuleFor(c => c.CustomerName, f => f.Name.FullName())
        .RuleFor(c => c.PhoneNumber, f => f.Phone.PhoneNumber("09########"))
        .RuleFor(c => c.TotalPurchased, 0); // Khởi tạo bằng 0, sẽ update sau logic

    public static Faker<User> UserFaker { get; } = new Faker<User>()
        .RuleFor(u => u.UserId, f => f.IndexFaker + 1)
        .RuleFor(u => u.UserFullName, f => f.Name.FullName())
        .RuleFor(u => u.Email, property => property.Internet.Email())
        .RuleFor(u => u.RoleId, f => f.Random.Int(1, 2))
        .RuleFor(u => u.IsActive, true);

    // 2. Hàm Generator thông minh để tạo Order khớp logic tính toán
    /// <summary>
    /// Tạo ra một Order hoàn chỉnh với OrderDetails khớp giá Product và TotalAmount được tính toán đúng.
    /// </summary>
    /// <param name="existingProducts">Danh sách sản phẩm có sẵn (để lấy giá và ID)</param>
    /// <param name="customerId">ID khách hàng</param>
    /// <param name="itemCount">Số lượng item trong đơn hàng</param>
    public static Order GenerateOrder(List<Product> existingProducts, int customerId, int itemCount = 2)
    {
        var faker = new Faker();

        // A. Chọn ngẫu nhiên sản phẩm từ danh sách có sẵn
        var selectedProducts = faker.PickRandom(existingProducts, itemCount).ToList();

        // B. Tạo OrderDetails dựa trên sản phẩm đã chọn
        var orderDetails = new List<OrderDetail>();
        foreach (var product in selectedProducts)
        {
            var qty = faker.Random.Int(1, 5);
            orderDetails.Add(new OrderDetail
            {
                ProductId = product.ProductId,
                Product = product, // Link reference để tiện test
                Quantity = qty,
                UnitPrice = product.SellPrice, // QUAN TRỌNG: Giá phải khớp Product
                // SubTotal là computed property trong Entity, không cần set, nhưng nếu mock object thì cẩn thận
            });
        }

        // C. Tính toán các con số tổng (Business Logic Simulation)
        var subTotal = orderDetails.Sum(od => od.Quantity * od.UnitPrice);

        // Random Discount: 50% cơ hội là giảm tiền mặt, 50% là giảm % (hoặc 0)
        decimal discount = 0;
        bool isPercentDiscount = faker.Random.Bool();

        if (isPercentDiscount)
            discount = Math.Round(faker.Random.Decimal(0, 0.2m), 2); // 0% - 20% (dạng < 1)
        else
            discount = faker.Random.Decimal(0, 50); // Giảm 0 - 50$ (dạng >= 1)

        // Tính Total Amount thực tế
        decimal totalAmount = 0;
        if (discount < 1 && discount > 0) // Logic phần trăm
        {
            totalAmount = subTotal - (subTotal * discount);
        }
        else // Logic tiền mặt
        {
            totalAmount = subTotal - discount;
        }

        if (totalAmount < 0) totalAmount = 0;

        // D. Tạo Order và gán dữ liệu
        var order = new Faker<Order>()
            .RuleFor(o => o.OrderId, f => f.IndexFaker + 1)
            .RuleFor(o => o.CustomerId, customerId) // Khớp Customer ID đầu vào
            .RuleFor(o => o.UserId, f => f.Random.Int(1, 10)) // có thể sai sót nhưng ko quan trọng
            .RuleFor(o => o.OrderDate, DateTime.UtcNow)
            .RuleFor(o => o.Status, OrderStatus.Pending)
            .RuleFor(o => o.SubtotalAmount, subTotal)
            .RuleFor(o => o.Discount, discount)
            .RuleFor(o => o.TotalAmount, totalAmount) // Đã tính đúng
            .RuleFor(o => o.OrderDetails, orderDetails)
            .Generate();

        // Gán ngược lại OrderId cho Details (nếu cần thiết cho EF Core navigation mock)
        foreach (var detail in order.OrderDetails!)
        {
            detail.OrderId = order.OrderId;
            detail.Order = order;
        }

        return order;
    }
}