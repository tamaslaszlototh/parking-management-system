using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.ParkingSpot;

namespace ParkingManagementSystem.Infrastructure.Persistence.Repositories;

public class ParkingSpotsRepository : IParkingSpotsRepository
{
    private readonly ParkingManagementSystemDbContext _dbContext;

    public ParkingSpotsRepository(ParkingManagementSystemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ParkingSpot parkingSpot)
    {
        await _dbContext.AddAsync(parkingSpot);
        await _dbContext.SaveChangesAsync();
    }
}