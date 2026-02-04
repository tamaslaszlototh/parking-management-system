using ParkingManagementSystem.Domain.ParkingSpot;
using ErrorOr;

namespace ParkingManagementSystem.Application.Common.Persistence.Interfaces;

public interface IParkingSpotsRepository
{
    Task AddAsync(ParkingSpot parkingSpot);
}