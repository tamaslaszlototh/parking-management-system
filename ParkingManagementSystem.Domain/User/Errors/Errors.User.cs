using ErrorOr;

namespace ParkingManagementSystem.Domain.User.Errors;

public static class Errors
{
    public static class User
    {
        public static Error UserAlreadyExists()
        {
            return Error.Conflict(code: "UserAlreadyExists",
                description: "User already exists with this email.");
        }
    }
}