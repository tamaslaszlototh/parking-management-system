namespace ParkingManagementSystem.Contracts.User.LoginUser;

public record LoginUserResponse(
    string Token,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    List<UserRole> Roles);