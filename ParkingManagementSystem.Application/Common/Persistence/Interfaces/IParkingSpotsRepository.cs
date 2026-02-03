using ParkingManagementSystem.Domain.AddParkingSpot;
using ErrorOr;

namespace ParkingManagementSystem.Application.Common.Persistence.Interfaces;

public interface IParkingSpotsRepository
{
    Task<ErrorOr<ParkingSpot>> AddAsync(ParkingSpot parkingSpot);
}