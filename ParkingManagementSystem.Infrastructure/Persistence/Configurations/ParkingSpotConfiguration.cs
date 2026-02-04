using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkingManagementSystem.Domain.ParkingSpot;
using ParkingManagementSystem.Domain.ParkingSpot.Enums;
using ParkingManagementSystem.Domain.ParkingSpot.ValueObjects;

namespace ParkingManagementSystem.Infrastructure.Persistence.Configurations;

public class ParkingSpotConfiguration : IEntityTypeConfiguration<ParkingSpot>
{
    public void Configure(EntityTypeBuilder<ParkingSpot> builder)
    {
        ConfigureParkingSpotTable(builder);
    }

    private static void ConfigureParkingSpotTable(EntityTypeBuilder<ParkingSpot> builder)
    {
        builder.ToTable("ParkingSpots");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(
                name => name.Value,
                name => ParkingSpotName.Create(name));

        builder.Property(p => p.Description)
            .IsRequired(false)
            .HasMaxLength(150)
            .HasConversion(
                description => description.Value,
                description => ParkingSpotDescription.Create(description));

        builder.Property(p => p.State)
            .IsRequired()
            .HasConversion(
                state => state.ToString(),
                state => Enum.Parse<ParkingSpotState>(state));
    }
}