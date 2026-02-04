using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Infrastructure.Persistence;
using ParkingManagementSystem.Infrastructure.Persistence.Repositories;

namespace ParkingManagementSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureLayer(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddDbContext<ParkingManagementSystemDbContext>(options =>
                options.UseInMemoryDatabase("parking-management-system"));
        }

        services.AddScoped<IParkingSpotsRepository, ParkingSpotsRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}