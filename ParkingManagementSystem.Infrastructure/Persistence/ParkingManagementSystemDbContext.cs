using Microsoft.EntityFrameworkCore;
using ParkingManagementSystem.Domain.AddParkingSpot;
using ParkingManagementSystem.Domain.Common.Interfaces;

namespace ParkingManagementSystem.Infrastructure.Persistence;

public class ParkingManagementSystemDbContext : DbContext
{
    public DbSet<ParkingSpot> ParkingSpots { get; set; }

    public ParkingManagementSystemDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Ignore<List<IDomainEvent>>()
            .ApplyConfigurationsFromAssembly(typeof(ParkingManagementSystemDbContext).Assembly);
    }
}