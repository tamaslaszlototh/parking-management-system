using ParkingManagementSystem.Domain.User;
using ParkingManagementSystem.Domain.User.ValueObjects;

namespace ParkingManagementSystem.Application.Common.Persistence.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken);
}