using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.Reservation;

namespace ParkingManagementSystem.Infrastructure.Persistence.Repositories;

public class ReservationsRepository : IReservationsRepository
{
    private readonly ParkingManagementSystemDbContext _dbContext;

    public ReservationsRepository(ParkingManagementSystemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken)
    {
        await _dbContext.Reservations.AddAsync(reservation, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasReservationForAsync(Guid userId, DateOnly date,
        CancellationToken cancellationToken)
    {
        return _dbContext.Reservations.Any(r => r.UserId == userId && r.ReservationDate == date);
    }
}