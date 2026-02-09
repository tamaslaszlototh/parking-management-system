using ErrorOr;

namespace ParkingManagementSystem.Domain.Reservation.Errors;

public static class Errors
{
    public static class Reservation
    {
        public static Error NotFoundFreeParkingSpot(DateOnly date)
        {
            return Error.NotFound(
                code: "NotFoundFreeParkingSpot",
                description: $"No free parking spot found for date: {date}.");
        }

        public static Error UserAlreadyHasReservationForDate(DateOnly date)
        {
            return Error.Conflict(
                code: "UserHasAlreadyReservationForDate",
                description: $"User already has reservation for date: {date}.");
        }
    }
}