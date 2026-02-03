using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ParkingManagementSystem.Application.Common.Behaviors;

namespace ParkingManagementSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(DependencyInjection));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}