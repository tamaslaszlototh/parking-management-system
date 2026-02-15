using ErrorOr;

namespace ParkingManagementSystem.Domain.ParkingSpot.Errors;

public static class Errors
{
    public static class ParkingSpot
    {
        public static Error ParkingSpotAlreadyReservedForDate(DateOnly date)
        {
            return Error.Conflict(
                code: "ParkingSpotAlreadyReserved",
                description: $"Parking spot is already reserved for this date {date}.");
        }

        public static Error ParkingSpotNotFound()
        {
            return Error.NotFound(
                code: "ParkingSpotNotFound",
                description: "Parking spot not found.");
        }

        public static Error ParkingSpotDeactivatedCannotBeReserved()
        {
            return Error.Forbidden(
                code: "ParkingSpotDeactivatedCannotBeReserved",
                description: "Parking spot is deactivated and cannot be reserved.");
        }

        public static Error ParkingSpotIsNotDedicated()
        {
            return Error.Forbidden(
                code: "ParkingSpotIsNotDedicated",
                description: "Parking spot is not dedicated.");
        }

        public static Error ParkingSpotIsAlreadyAssigned()
        {
            return Error.Conflict(
                code: "ParkingSpotIsAlreadyAssigned",
                description: "Parking spot is already assigned to a manager.");
        }
        
        public static Error ParkingSpotIsAlreadyDeactivated()
        {
            return Error.Conflict(
                code: "ParkingSpotIsAlreadyDeactivated",
                description: "Parking spot is already deactivated.");
        }
    }
}