using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Interfaces;
using TechHaven.Infrastructure.Persistence;
using TechHaven.Infrastructure.Persistence.Repositories;
using TechHaven.Infrastructure.Services;

namespace TechHaven.Infrastructure;

/// <summary>
/// Extension methods for configuring Infrastructure services in Dependency Injection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure services to the specified <see cref="IServiceCollection"/>.
    /// This includes database context, Unit of Work, and repositories.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found in configuration.");

            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    // Specify migrations assembly
                    npgsqlOptions.MigrationsAssembly("TechHaven.Infrastructure");

                    // Enable retry on failure for resilience
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);

                    // Command timeout (30 seconds)
                    npgsqlOptions.CommandTimeout(30);
                })
                // Use snake_case naming convention for PostgreSQL
                .UseSnakeCaseNamingConvention()
                // Enable sensitive data logging in development only
                .EnableSensitiveDataLogging(
                    configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging", false))
                // Enable detailed errors in development only
                .EnableDetailedErrors(
                    configuration.GetValue<bool>("Logging:EnableDetailedErrors", false));
        });

        // ============================================
        // 2. Unit of Work Registration
        // ============================================
        // CRITICAL: Must be Scoped to ensure one instance per request
        // services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ============================================
        // 3. Individual Repository Registration (Optional)
        // ============================================
        // Uncomment if you need to inject individual repositories
        // However, it's recommended to only inject IUnitOfWork
        /*
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ICommissionRepository, CommissionRepository>();
        services.AddScoped<IAppSettingRepository, AppSettingRepository>();
        */

        // ============================================
        // 4. Application Services
        // ============================================

        // ============================================
        // 5. Memory Cache for OTP
        // ============================================

        // ============================================
        // 6. JWT Authentication
        // ============================================

        return services;
    }
}
