using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace TechHaven.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Đăng ký AutoMapper với tất cả Profiles trong Assembly hiện tại
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        // Đăng ký các services khác ở đây
        
        return services;
    }
}