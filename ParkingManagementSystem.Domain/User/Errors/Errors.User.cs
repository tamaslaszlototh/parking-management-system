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

        public static Error UserNotFound()
        {
            return Error.NotFound(
                code: "UserNotFound",
                description: "User not found.");
        }

        public static Error ManagerNotFound()
        {
            return Error.NotFound(
                code: "ManagerNotFound",
                description: "Manager not found.");
        }

        public static Error ManagerIsAlreadyAssignedToParkingSpot()
        {
            return Error.NotFound(
                code: "ManagerIsAlreadyAssignedToParkingSpot",
                description: "Manager is already assigned to a parking spot.");
        }
    }
}