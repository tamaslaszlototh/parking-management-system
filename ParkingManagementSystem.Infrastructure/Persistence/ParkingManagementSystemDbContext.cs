using Microsoft.EntityFrameworkCore;
using ParkingManagementSystem.Domain.ParkingSpot;
using ParkingManagementSystem.Domain.Common.Interfaces;
using ParkingManagementSystem.Domain.Reservation;
using ParkingManagementSystem.Domain.User;

namespace ParkingManagementSystem.Infrastructure.Persistence;

public class ParkingManagementSystemDbContext : DbContext
{
    public DbSet<ParkingSpot> ParkingSpots { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

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