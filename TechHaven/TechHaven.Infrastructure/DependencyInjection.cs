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
using TechHaven.Infrastructure.Configuration;
using TechHaven.Infrastructure.Authorization;
using TechHaven.Infrastructure.Authorization.Handlers;
using Microsoft.AspNetCore.Authorization;
using TechHaven.Infrastructure.Services.AI;
using TechHaven.Infrastructure.Services.Email;

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
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ============================================
        // 3. Individual Repository Registration (Optional)
        // ============================================
        // Uncomment if you need to inject individual repositories
        // However, it's recommended to only inject IUnitOfWork
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ICommissionRepository, CommissionRepository>();
        services.AddScoped<IAppSettingRepository, AppSettingRepository>();

        // ============================================
        // 4. Application Services
        // ============================================
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IOtpService, OtpService>();
        // services.AddScoped<IEmailService, EmailService>();

        // ============================================
        // Configure Brevo Email Service
        // ============================================
        services.Configure<BrevoSettings>(options =>
        {
            options.ApiKey = Environment.GetEnvironmentVariable("BREVO_API_KEY") ?? "";
            options.SenderEmail = Environment.GetEnvironmentVariable("BREVO_SENDER_EMAIL") ?? "";
            options.SenderName = Environment.GetEnvironmentVariable("BREVO_SENDER_NAME") ?? "TechHaven";
        });

        // Register Brevo Email Service
        services.AddScoped<IEmailService, BrevoEmailService>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // ============================================
        // 5. Memory Cache for OTP
        // ============================================
        services.AddMemoryCache();

        // ============================================
        // 6. JWT Authentication
        // ============================================
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey not configured");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization(options =>
        {
            // Role-based policies
            options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
                policy.RequireRole(Roles.Admin));

            options.AddPolicy(AuthorizationPolicies.SellerOnly, policy =>
                policy.RequireRole(Roles.Seller));

            options.AddPolicy(AuthorizationPolicies.AdminOrSeller, policy =>
                policy.RequireRole(Roles.Admin, Roles.Seller));

            // Permission-based policies
            options.AddPolicy(AuthorizationPolicies.ManageProducts, policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole(Roles.Admin) ||
                    context.User.IsInRole(Roles.Seller)));

            options.AddPolicy(AuthorizationPolicies.ManageOrders, policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole(Roles.Admin) ||
                    context.User.IsInRole(Roles.Seller)));

            options.AddPolicy(AuthorizationPolicies.ManageCustomers, policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole(Roles.Admin) ||
                    context.User.IsInRole(Roles.Seller)));

            options.AddPolicy(AuthorizationPolicies.ManageUsers, policy =>
                policy.RequireRole(Roles.Admin));

            options.AddPolicy(AuthorizationPolicies.ViewReports, policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole(Roles.Admin) ||
                    context.User.IsInRole(Roles.Seller)));

            options.AddPolicy(AuthorizationPolicies.ManageSettings, policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole(Roles.Admin) ||
                    context.User.IsInRole(Roles.Seller)));

            // Resource-based policy
            options.AddPolicy("SameUserPolicy", policy =>
                policy.Requirements.Add(new SameUserRequirement()));

            options.AddPolicy("AppSettingAccess", policy =>
                policy.Requirements.Add(new AppSettingAccessRequirement()));
        });

        // Register Authorization Handlers
        services.AddScoped<IAuthorizationHandler, SameUserAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, AppSettingAuthorizationHandler>();

        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        // Configure SMTP Settings
        services.Configure<SmtpSettings>(
            configuration.GetSection("SmtpSettings"));

        // Register Email Service
        services.AddScoped<IEmailService, BrevoEmailService>();

        // 7. Supabase Storage Configuration
        services.Configure<SupabaseSettings>(options =>
        {
            options.Url = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? "";
            options.ApiKey = Environment.GetEnvironmentVariable("SUPABASE_API_KEY") ?? "";
            options.BucketName = Environment.GetEnvironmentVariable("SUPABASE_BUCKET_NAME") ?? "";
            options.MaxFileSizeBytes = configuration.GetSection("SupabaseSettings").GetValue<long>("MaxFileSizeBytes", 5242880);
            options.AllowedExtensions = configuration.GetSection("SupabaseSettings").GetSection("AllowedExtensions").Get<string[]>() ?? new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        });

        // Register Image Upload Service
        services.AddScoped<IImageUploadService, SupabaseImageUploadService>();

        // Configure Gemini
        services.Configure<GeminiSettings>(configuration.GetSection("GeminiSettings"));
        services.AddScoped<RAGService>();
        services.AddScoped<IAIChatService, GeminiAIChatService>();

        // // Configure OpenAI Settings
        // services.Configure<OpenAISettings>(configuration.GetSection("OpenAISettings"));
        // services.AddScoped<RAGService>();
        // services.AddScoped<IAIChatService, OpenAIChatService>();

        return services;
    }
}
