using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkingManagementSystem.Domain.User;
using ParkingManagementSystem.Domain.User.Enums;
using ParkingManagementSystem.Domain.User.ValueObjects;

namespace ParkingManagementSystem.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        ConfigureUserTable(builder);
    }

    private static void ConfigureUserTable(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(
                name => name.Value,
                name => UserName.Create(name));

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(
                name => name.Value,
                name => UserName.Create(name));

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasConversion(
                email => email.Value,
                email => Email.Create(email));

        builder.Property(u => u.Phone)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(
                phone => phone.Value,
                phone => Phone.Create(phone));

        builder.Property(u => u.Password)
            .IsRequired()
            .HasConversion(
                password => password.Value,
                password => Password.Create(password));

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion(
                role => role.ToString(),
                role => Enum.Parse<UserRole>(role));

        builder.HasIndex(u => u.Email);
    }
}