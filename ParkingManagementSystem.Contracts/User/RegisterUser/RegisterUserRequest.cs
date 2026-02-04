namespace ParkingManagementSystem.Contracts.User.RegisterUser;

public record RegisterUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Password,
    UserRole? Role);