using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkingManagementSystem.Domain.Reservation;

namespace ParkingManagementSystem.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        ConfigureReservationTable(builder);
    }

    private static void ConfigureReservationTable(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");

        builder.HasKey(r => r.Id);

        builder.Property(x => x.ParkingSpotId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ReservationDate)
            .IsRequired();

        builder.HasIndex(x => new { x.ParkingSpotId, x.ReservationDate });
    }
}