using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ParkingManagementSystem.Api.Filters;

public class LoggingActionFilter : IResultFilter
{
    private readonly ILogger<LoggingActionFilter> _logger;
    private Stopwatch? _stopwatch;

    public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        _stopwatch = Stopwatch.StartNew();

        var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString() ?? "unknown";
        var userId = context.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? "anonymous";
        var controller = context.RouteData.Values["controller"];
        var action = context.RouteData.Values["action"];

        _logger.LogInformation(
            "[CorrelationId: {CorrelationId}] {Controller}.{Action} started. UserId: {UserId}",
            correlationId, controller, action, userId);
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        _stopwatch?.Stop();

        var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString() ?? "unknown";
        var controller = context.RouteData.Values["controller"];
        var action = context.RouteData.Values["action"];
        var statusCode = context.HttpContext.Response.StatusCode;
        var result = statusCode is >= 200 and < 300 ? "Success" : "Failed";

        _logger.LogInformation(
            "[CorrelationId: {CorrelationId}] {Controller}.{Action} {Result} with {StatusCode} in {ElapsedMs}ms",
            correlationId, controller, action, result, statusCode, _stopwatch?.ElapsedMilliseconds);
    }
}