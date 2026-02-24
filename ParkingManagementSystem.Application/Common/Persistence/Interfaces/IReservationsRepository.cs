using ParkingManagementSystem.Domain.Reservation;

namespace ParkingManagementSystem.Application.Common.Persistence.Interfaces;

public interface IReservationsRepository
{
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken);
    Task<bool> HasReservationForAsync(Guid userId, DateOnly date, CancellationToken cancellationToken);
    Task<List<Guid>> GetReservedParkingSpotsForDateAsync(DateOnly date, CancellationToken cancellationToken);
    Task<List<Reservation>> GetActiveReservationsAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<Reservation>> GetReservationsByIdsAsync(List<Guid> reservationIds, CancellationToken cancellationToken);

    Task<List<Reservation>> GetReservationsForParkingSpotFromTodayAsync(Guid parkingSpotId,
        CancellationToken cancellationToken);

    void Remove(Reservation reservation);

    Task<List<Reservation>> GetActiveReservationsForDatesAsync(List<DateOnly> dates,
        CancellationToken cancellationToken);
}