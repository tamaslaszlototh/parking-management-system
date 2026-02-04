using ErrorOr;
using ParkingManagementSystem.Domain.User;
using ParkingManagementSystem.Domain.User.ValueObjects;

namespace ParkingManagementSystem.Application.Common.Persistence.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<User?> GetByEmailAsync(Email email);
}