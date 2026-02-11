using MediatR;
using Microsoft.AspNetCore.Http;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.Common.Interfaces;

namespace ParkingManagementSystem.Infrastructure.Middlewares;

public class EventualConsistencyMiddleware
{
    public const string DomainEventsKey = "DomainEventsKey";

    private readonly RequestDelegate _next;

    public EventualConsistencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IPublisher publisher, IUnitOfWork unitOfWork)
    {
        context.Response.OnCompleted(async () =>
        {
            await unitOfWork.BeginTransactionAsync();
            
            try
            {
                if (context.Items.TryGetValue(DomainEventsKey, out var value) &&
                    value is Queue<IDomainEvent> domainEvents)
                {
                    while (domainEvents.TryDequeue(out var nextEvent))
                    {
                        await publisher.Publish(nextEvent);
                    }
                }

                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                //Todo: add resiliency
            }
            finally
            {
                await unitOfWork.RollbackTransactionAsync();
            }
        });

        await _next(context);
    }
}