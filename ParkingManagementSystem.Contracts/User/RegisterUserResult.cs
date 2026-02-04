namespace ParkingManagementSystem.Contracts.User;

public record RegisterUserResult(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    UserRole Role);