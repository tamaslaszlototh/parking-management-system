using ParkingManagementSystem.Domain.ParkingSpot;
using ErrorOr;

namespace ParkingManagementSystem.Application.Common.Persistence.Interfaces;

public interface IParkingSpotsRepository
{
    Task<ErrorOr<ParkingSpot>> AddAsync(ParkingSpot parkingSpot);
}