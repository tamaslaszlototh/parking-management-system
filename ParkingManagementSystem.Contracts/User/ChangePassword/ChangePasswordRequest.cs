namespace ParkingManagementSystem.Contracts.User.ChangePassword;

public record ChangePasswordRequest(Guid UserId, string CurrentPassword, string NewPassword);