using ParkingManagementSystem.Domain.ParkingSpot;

namespace ParkingManagementSystem.Application.Common.Persistence.Interfaces;

public interface IParkingSpotsRepository
{
    Task AddAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken);
    Task<ParkingSpot?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<ParkingSpot>> GetNotDeactivatedParkingSpotsAsync(CancellationToken cancellationToken);

    Task<bool> FreeForReservationFor(DateOnly date, Guid userId, Guid parkingSpotId,
        CancellationToken cancellationToken);
}