using ErrorOr;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.AddParkingSpot;

namespace ParkingManagementSystem.Infrastructure.Persistence.Repositories;

public class ParkingSpotsRepository : IParkingSpotsRepository
{
    private readonly ParkingManagementSystemDbContext _dbContext;

    public ParkingSpotsRepository(ParkingManagementSystemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<ParkingSpot>> AddAsync(ParkingSpot parkingSpot)
    {
        await _dbContext.AddAsync(parkingSpot);
        await _dbContext.SaveChangesAsync();
        return parkingSpot;
    }
}