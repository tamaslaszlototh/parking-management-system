using Microsoft.EntityFrameworkCore;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.Reservation;

namespace ParkingManagementSystem.Infrastructure.Persistence.Repositories;

public class ReservationsRepository : IReservationsRepository
{
    private readonly ParkingManagementSystemDbContext _dbContext;

    public ReservationsRepository(ParkingManagementSystemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken)
    {
        await _dbContext.Reservations.AddAsync(reservation, cancellationToken);
    }

    public async Task<bool> HasReservationForAsync(Guid userId, DateOnly date,
        CancellationToken cancellationToken)
    {
        return _dbContext.Reservations.Any(r => r.UserId == userId && r.ReservationDate == date);
    }

    public async Task<List<Guid>> GetReservedParkingSpotsForDateAsync(DateOnly date,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations
            .Where(r => r.ReservationDate == date)
            .Select(r => r.ParkingSpotId)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<Reservation>> GetActiveReservationsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations
            .Where(r => r.ReservationDate >= DateOnly.FromDateTime(DateTime.UtcNow) && r.UserId == userId)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<Reservation>> GetReservationsByIdsAsync(List<Guid> reservationIds,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations.Where(r => reservationIds.Contains(r.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Reservation>> GetReservationsForParkingSpotFromTodayAsync(Guid parkingSpotId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations.Where(r =>
                r.ParkingSpotId == parkingSpotId && r.ReservationDate >= DateOnly.FromDateTime(DateTime.UtcNow))
            .ToListAsync(cancellationToken);
    }

    public void Remove(Reservation reservation)
    {
        _dbContext.Reservations.Remove(reservation);
    }

    public async Task<List<Reservation>> GetActiveReservationsForDatesAsync(List<DateOnly> dates,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations.Where(r => dates.Contains(r.ReservationDate))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Reservation>> GetReservationsForUserAsync(Guid userId, bool includeExpired,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations
            .Where(r =>
                r.UserId == userId && (includeExpired || r.ReservationDate >= DateOnly.FromDateTime(DateTime.UtcNow)))
            .OrderBy(r => r.ReservationDate)
            .ToListAsync(cancellationToken);
    }
}