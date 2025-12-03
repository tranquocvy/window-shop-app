using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Enums;
using TechHaven.Infrastructure.Persistence;

namespace TechHaven.Presentation.WebAPI.Seeders;

/// <summary>
/// Seeds demo <see cref="Order"/>, <see cref="OrderDetail"/> and <see cref="Payment"/> data
/// based on existing <see cref="Product"/> and <see cref="User"/> entities.
/// 
/// The generated data is used to power the Dashboard and Reporting features:
/// - Today order count & revenue
/// - Top selling products
/// - Recent orders list
/// - Monthly revenue chart
/// </summary>
public class OrderSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderSeeder> _logger;

    private const string SeedNotePrefix = "[SEED-DEMO]";

    public OrderSeeder(
        AppDbContext context,
        ILogger<OrderSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seed demo orders each time the application starts.
    /// Existing seeded orders (identified by <see cref="SeedNotePrefix"/>) will be removed
    /// so that new random demo data is generated on every run.
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        // Kiểm tra đã có order nào của hôm nay chưa
        var hasTodayOrder = await _context.Orders
            .AnyAsync(o => o.OrderDate.Date == today, cancellationToken);

        if (hasTodayOrder)
        {
            _logger.LogInformation("OrderSeeder: Today's orders already exist ({Today}), skipping seed.", today);
            return;
        }

        // Ensure we have required master data
        var products = await _context.Products
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var users = await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .ToListAsync(cancellationToken);

        var customers = await _context.Customers
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (products.Count == 0 || users.Count == 0)
        {
            _logger.LogWarning(
                "Skipping OrderSeeder: Products ({ProductCount}) or Users ({UserCount}) not found.",
                products.Count,
                users.Count);
            return;
        }

        var executionStrategy = _context.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            // 1. Remove old seeded demo orders (and related details/payments)
            var oldSeedOrders = await _context.Orders
                .Where(o => o.Notes != null && o.Notes.StartsWith(SeedNotePrefix))
                .ToListAsync(cancellationToken);

            if (oldSeedOrders.Count > 0)
            {
                var oldOrderIds = oldSeedOrders
                    .Select(o => o.OrderId)
                    .ToList();

                var oldOrderDetails = await _context.OrderDetails
                    .Where(od => oldOrderIds.Contains(od.OrderId))
                    .ToListAsync(cancellationToken);

                var oldPayments = await _context.Payments
                    .Where(p => oldOrderIds.Contains(p.OrderId))
                    .ToListAsync(cancellationToken);

                _context.OrderDetails.RemoveRange(oldOrderDetails);
                _context.Payments.RemoveRange(oldPayments);
                _context.Orders.RemoveRange(oldSeedOrders);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Removed {OrderCount} previously seeded demo orders.",
                    oldSeedOrders.Count);
            }

            // 2. Generate fresh demo orders
            var newOrders = new List<Order>();
            var newOrderDetails = new List<OrderDetail>();
            var newPayments = new List<Payment>();

            var random = new Random();
            var utcNow = DateTime.UtcNow;
            var todayDate = utcNow.Date;

            // Helper to get a random seller, fallback to any user
            var sellerUsers = users
                .Where(u => u.Role != null && u.Role.RoleName == "Seller")
                .ToList();

            User GetRandomUser()
            {
                if (sellerUsers.Count > 0)
                {
                    return sellerUsers[random.Next(sellerUsers.Count)];
                }

                return users[random.Next(users.Count)];
            }

            Customer? GetRandomCustomerOrNull()
            {
                // ~80% orders have a registered customer, 20% are walk-in
                if (customers.Count == 0 || random.NextDouble() < 0.2)
                {
                    return null;
                }

                return customers[random.Next(customers.Count)];
            }

            Product GetRandomProduct()
            {
                return products[random.Next(products.Count)];
            }

            // 2.1 Create today's demo orders (for dashboard "Today" stats)
            var todayOrderCount = random.Next(8, 18); // between 8 and 17 orders today
            for (int i = 0; i < todayOrderCount; i++)
            {
                var order = BuildRandomOrder(
                    GetRandomUser(),
                    GetRandomCustomerOrNull(),
                    todayDate.AddHours(random.Next(9, 21)) // 9:00 - 21:00
                );

                newOrders.Add(order);
                BuildDetailsAndPaymentsForOrder(order, GetRandomProduct, random, newOrderDetails, newPayments);
            }

            // 2.2 Create historical orders (for report & monthly revenue)
            // Generate data for the last 90 days
            var historyDays = 90;
            for (int dayOffset = 1; dayOffset <= historyDays; dayOffset++)
            {
                var targetDate = todayDate.AddDays(-dayOffset);

                // 0 - 12 orders per day, with some days having 0 orders
                var dailyOrderCount = random.Next(0, 13);
                for (int i = 0; i < dailyOrderCount; i++)
                {
                    var order = BuildRandomOrder(
                        GetRandomUser(),
                        GetRandomCustomerOrNull(),
                        targetDate.AddHours(random.Next(9, 21))
                    );

                    newOrders.Add(order);
                    BuildDetailsAndPaymentsForOrder(order, GetRandomProduct, random, newOrderDetails, newPayments);
                }
            }

            await _context.Orders.AddRangeAsync(newOrders, cancellationToken);
            await _context.OrderDetails.AddRangeAsync(newOrderDetails, cancellationToken);
            await _context.Payments.AddRangeAsync(newPayments, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Seeded {OrderCount} demo orders, {DetailCount} order details and {PaymentCount} payments.",
                newOrders.Count,
                newOrderDetails.Count,
                newPayments.Count);
        });
    }

    private static Order BuildRandomOrder(
        User user,
        Customer? customer,
        DateTime dateTimeLocalOrUtc)
    {
        // Ensure UTC for PostgreSQL / Supabase
        var utcDateTime = dateTimeLocalOrUtc.Kind == DateTimeKind.Utc
            ? dateTimeLocalOrUtc
            : DateTime.SpecifyKind(dateTimeLocalOrUtc, DateTimeKind.Utc);

        // Weighted status: most orders Completed, some Pending/Processing/Cancelled
        var status = GetRandomStatus();

        return new Order
        {
            UserId = user.UserId,
            CustomerId = customer?.CustomerId,
            OrderDate = utcDateTime,
            Status = status,
            SubtotalAmount = 0, // will be calculated after adding details
            Discount = 0,
            TotalAmount = 0,    // will be calculated after adding details
            Notes = $"{SeedNotePrefix} Demo order for dashboard & reports"
        };
    }

    private static OrderStatus GetRandomStatus()
    {
        var random = new Random();
        var roll = random.NextDouble();

        // ~70% Completed, 15% Pending, 10% Processing, 5% Cancelled
        if (roll < 0.7) return OrderStatus.Completed;
        if (roll < 0.85) return OrderStatus.Pending;
        if (roll < 0.95) return OrderStatus.Processing;
        return OrderStatus.Cancelled;
    }

    private static void BuildDetailsAndPaymentsForOrder(
        Order order,
        Func<Product> getRandomProduct,
        Random random,
        List<OrderDetail> detailsCollector,
        List<Payment> paymentsCollector)
    {
        // 1-4 different products per order
        var lineCount = random.Next(1, 5);
        var usedProductIds = new HashSet<int>();

        decimal subtotal = 0;

        for (int i = 0; i < lineCount; i++)
        {
            var product = getRandomProduct();
            if (!usedProductIds.Add(product.ProductId))
            {
                // avoid duplicate product lines; try a couple more times
                int attempts = 0;
                while (attempts < 3 && usedProductIds.Contains(product.ProductId))
                {
                    product = getRandomProduct();
                    attempts++;
                }

                if (!usedProductIds.Add(product.ProductId))
                {
                    continue;
                }
            }

            var quantity = random.Next(1, 4); // 1-3 units
            var unitPrice = product.SellPrice;

            var detail = new OrderDetail
            {
                Order = order,
                ProductId = product.ProductId,
                Quantity = quantity,
                UnitPrice = unitPrice
            };

            subtotal += quantity * unitPrice;
            detailsCollector.Add(detail);
        }

        if (subtotal <= 0)
        {
            // no valid details created; mark as cancelled & skip payment
            order.Status = OrderStatus.Cancelled;
            order.SubtotalAmount = 0;
            order.TotalAmount = 0;
            return;
        }

        // Apply a small random discount (0-10%)
        var discountRate = (decimal)random.NextDouble() * 0.1m;
        var discount = Math.Round(subtotal * discountRate, 2, MidpointRounding.AwayFromZero);

        var total = subtotal - discount;

        order.SubtotalAmount = subtotal;
        order.Discount = discount;
        order.TotalAmount = total;

        // Only create payments for completed orders
        if (order.Status == OrderStatus.Completed)
        {
            // Some orders might be partially paid (90-100%)
            var paidRate = 0.9m + (decimal)random.NextDouble() * 0.1m;
            var paidAmount = Math.Round(total * paidRate, 2, MidpointRounding.AwayFromZero);

            var payment = new Payment
            {
                Order = order,
                Amount = paidAmount,
                PaymentDate = order.OrderDate.AddMinutes(random.Next(5, 180)),
                PaymentMethod = GetRandomPaymentMethod(random)
            };

            paymentsCollector.Add(payment);
        }
    }

    private static PaymentMethod GetRandomPaymentMethod(Random random)
    {
        var roll = random.NextDouble();

        if (roll < 0.6) return PaymentMethod.Cash;
        if (roll < 0.85) return PaymentMethod.BankTransfer;
        return PaymentMethod.CreditCard;
    }
}


