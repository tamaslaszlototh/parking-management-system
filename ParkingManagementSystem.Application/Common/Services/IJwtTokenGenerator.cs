using ParkingManagementSystem.Domain.User;

namespace ParkingManagementSystem.Application.Common.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}