using System.Reflection;
using Mapster;
using MapsterMapper;
using ParkingManagementSystem.Application;
using ParkingManagementSystem.Domain;
using ParkingManagementSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services
    .AddApplicationLayer()
    .AddDomainLayer()
    .AddInfrastructureLayer(builder.Configuration);

var config = TypeAdapterConfig.GlobalSettings;
config.Scan(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();