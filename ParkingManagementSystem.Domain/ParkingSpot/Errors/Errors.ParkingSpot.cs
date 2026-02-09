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
    }
}