using ParkingManagementSystem.Domain.Reservation;

namespace ParkingManagementSystem.Application.Common.Persistence.Interfaces;

public interface IReservationsRepository
{
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken);
    Task<bool> HasReservationForAsync(Guid parkingSpotId, DateOnly date, CancellationToken cancellationToken);
}