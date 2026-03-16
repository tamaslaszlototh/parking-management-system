using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ParkingManagementSystem.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IHttpContextAccessor _context;

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "CurrentPassword", "NewPassword", "Token"
    };

    private const int MaxPropertiesToLog = 5;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, IHttpContextAccessor context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = _context.HttpContext.Items["CorrelationId"]?.ToString() ?? "unknown";
        var userId = _context.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? "anonymous";

        _logger.LogInformation(
            "[CorrelationId: {CorrelationId}] Handling {RequestName}. UserId: {UserId}. Request: {RequestProperties}",
            correlationId, typeof(TRequest).Name, userId, GetRequestPropertiesJson(request));

        try
        {
            var response = await next(cancellationToken);

            stopwatch.Stop();

            if (response.IsError)
            {
                _logger.LogError(
                    "[CorrelationId: {CorrelationId}] {RequestName} failed after {ElapsedMs}ms with errors: {Errors}",
                    correlationId, typeof(TRequest).Name, stopwatch.ElapsedMilliseconds,
                    response.Errors?.Select(e => e.Description));
            }
            else
            {
                _logger.LogInformation(
                    "[CorrelationId: {CorrelationId}] {RequestName} handled in {ElapsedMs}ms",
                    correlationId, typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "[CorrelationId: {CorrelationId}] Failed to handle {RequestName} after {ElapsedMs}ms",
                correlationId, typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    private static string GetRequestPropertiesJson(TRequest request)
    {
        var type = typeof(TRequest);
        var properties = PropertyCache.GetOrAdd(type, t =>
            t.GetProperties()
                .Where(p => !SensitiveProperties.Any(s => p.Name.Contains(s, StringComparison.OrdinalIgnoreCase)))
                .Take(MaxPropertiesToLog)
                .ToArray());

        var dict = properties.ToDictionary(p => p.Name, p => GetPropertyValue(p.GetValue(request)));
        return JsonSerializer.Serialize(dict);
    }

    private static object GetPropertyValue(object? value)
    {
        return value switch
        {
            null => "null",
            IEnumerable<object> list => list.Select(GetPropertyValue).ToList(),
            _ => value
        };
    }
}