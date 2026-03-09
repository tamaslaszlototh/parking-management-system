using System.Reflection;
using Mapster;
using MapsterMapper;
using ParkingManagementSystem.Api.Filters;
using ParkingManagementSystem.Application;
using ParkingManagementSystem.Domain;
using ParkingManagementSystem.Infrastructure;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<LoggingActionFilter>();
builder.Services.AddControllers(options => { options.Filters.AddService<LoggingActionFilter>(); });
builder.Services.AddOpenApi();
builder.Services
    .AddApplicationLayer()
    .AddDomainLayer()
    .AddInfrastructureLayer(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var config = TypeAdapterConfig.GlobalSettings;
config.Scan(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .CreateLogger();
builder.Host.UseSerilog();

try
{
    Log.Information("Starting Parking Management System");

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseCors("AllowAngularApp");
    app.AddInfrastructureMiddleware();
    app.UseHttpsRedirection();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
finally
{
    Log.CloseAndFlush();
}