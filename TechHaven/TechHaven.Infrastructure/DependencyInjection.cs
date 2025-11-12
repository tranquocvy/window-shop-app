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

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                o => o.MigrationsAssembly("TechHaven.Infrastructure")
            ).UseSnakeCaseNamingConvention()
        );

        // Unit of Work

        // Application Services

        // Memory Cache for OTP

        // JWT Authentication

        return services;
    }
}
