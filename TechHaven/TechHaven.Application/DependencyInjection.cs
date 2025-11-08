using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TechHaven.Application.Common.Behaviors;

namespace TechHaven.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        // Đăng ký AutoMapper với tất cả Profiles trong Assembly hiện tại
        services.AddAutoMapper(assembly);
        
        // Đăng ký MediatR với tất cả handlers trong Assembly
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });
        
        // Đăng ký FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);
        
        // Đăng ký Pipeline Behaviors (thứ tự quan trọng - chạy theo thứ tự đăng ký)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        return services;
    }
}