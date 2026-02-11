using Microsoft.EntityFrameworkCore;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.ParkingSpot;
using ParkingManagementSystem.Domain.ParkingSpot.Enums;

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
    }

    public async Task<ParkingSpot?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.ParkingSpots.FirstOrDefaultAsync(p => p.Id == id, cancellationToken: cancellationToken);
    }

    public async Task<List<ParkingSpot>> GetNotDeactivatedParkingSpotsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.ParkingSpots.Where(p => p.State != ParkingSpotState.Deactivated)
            .ToListAsync(cancellationToken);
    }

    public void Update(ParkingSpot parkingSpot)
    {
        _dbContext.Update(parkingSpot);
    }
}