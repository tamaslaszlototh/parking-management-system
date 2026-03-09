using Microsoft.AspNetCore.Builder;
using ParkingManagementSystem.Infrastructure.Middlewares;

namespace ParkingManagementSystem.Infrastructure;

public static class RequestPipeline
{
    public static IApplicationBuilder AddInfrastructureMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<EventualConsistencyMiddleware>();
        return app;
    }
}