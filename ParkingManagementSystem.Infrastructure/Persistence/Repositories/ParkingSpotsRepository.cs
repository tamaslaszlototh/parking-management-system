using Microsoft.EntityFrameworkCore;
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

    public async Task AddAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        await _dbContext.AddAsync(parkingSpot, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ParkingSpot?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.ParkingSpots.FirstOrDefaultAsync(p => p.Id == id, cancellationToken: cancellationToken);
    }
}