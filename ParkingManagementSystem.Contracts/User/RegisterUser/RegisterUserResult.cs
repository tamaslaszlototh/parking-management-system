namespace ParkingManagementSystem.Contracts.User.RegisterUser;

public record RegisterUserResult(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    UserRole Role);