namespace ParkingManagementSystem.Contracts.User;

public record RegisterUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Password,
    UserRole? Role);