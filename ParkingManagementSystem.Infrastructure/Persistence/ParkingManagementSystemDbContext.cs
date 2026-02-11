using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ParkingManagementSystem.Domain.Common;
using ParkingManagementSystem.Domain.ParkingSpot;
using ParkingManagementSystem.Domain.Common.Interfaces;
using ParkingManagementSystem.Domain.Reservation;
using ParkingManagementSystem.Domain.User;
using ParkingManagementSystem.Infrastructure.Middlewares;

namespace ParkingManagementSystem.Infrastructure.Persistence;

public class ParkingManagementSystemDbContext : DbContext
{
    public DbSet<ParkingSpot> ParkingSpots { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

    private readonly IHttpContextAccessor _httpContextAccessor;

    public ParkingManagementSystemDbContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor) :
        base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Ignore<List<IDomainEvent>>()
            .ApplyConfigurationsFromAssembly(typeof(ParkingManagementSystemDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker.Entries<AggregateRoot>()
            .Select(entry => entry.Entity.PopDomainEvents())
            .SelectMany(x => x)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        Queue<IDomainEvent> domainEventQueue =
            _httpContextAccessor.HttpContext!.Items.TryGetValue(EventualConsistencyMiddleware.DomainEventsKey,
                out var value) && value is Queue<IDomainEvent> existingDomainEvents
                ? existingDomainEvents
                : new Queue<IDomainEvent>();

        domainEvents.ForEach(domainEventQueue.Enqueue);
        _httpContextAccessor.HttpContext.Items[EventualConsistencyMiddleware.DomainEventsKey] = domainEventQueue;
        return result;
    }
}