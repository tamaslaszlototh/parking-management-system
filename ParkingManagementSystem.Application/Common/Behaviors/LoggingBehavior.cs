using System.Diagnostics;
using System.Security.Claims;
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
            "[CorrelationId: {CorrelationId}] Handling {RequestName}. UserId: {UserId}",
            correlationId, typeof(TRequest).Name, userId);

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
}