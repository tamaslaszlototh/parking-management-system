using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Application.Common.Services;
using ParkingManagementSystem.Infrastructure.Authentication;
using ParkingManagementSystem.Infrastructure.Persistence;
using ParkingManagementSystem.Infrastructure.Persistence.Repositories;
using ParkingManagementSystem.Infrastructure.Services;

namespace ParkingManagementSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureLayer(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        if (environment.IsDevelopment())
        {
            services.AddDbContext<ParkingManagementSystemDbContext>(options =>
                options.UseInMemoryDatabase("parking-management-system"));
        }

        services.AddAuthentication(configuration);

        services.AddScoped<IParkingSpotsRepository, ParkingSpotsRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordService, BCryptPasswordService>();

        return services;
    }

    private static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.SectionName, jwtSettings);

        services.AddSingleton(Options.Create(jwtSettings));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret))
            });

        return services;
    }
}