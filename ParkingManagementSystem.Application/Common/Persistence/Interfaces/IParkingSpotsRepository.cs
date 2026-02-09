using ParkingManagementSystem.Domain.ParkingSpot;

namespace ParkingManagementSystem.Application.Common.Persistence.Interfaces;

public interface IParkingSpotsRepository
{
    Task AddAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken);
    Task<ParkingSpot?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}