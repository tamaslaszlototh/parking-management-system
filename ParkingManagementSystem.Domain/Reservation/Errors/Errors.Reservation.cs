using ErrorOr;

namespace ParkingManagementSystem.Domain.Reservation.Errors;

public static class Errors
{
    public static class Reservation
    {
        public static Error NotFoundFreeParkingSpotForDates(List<DateOnly> dates)
        {
            return Error.NotFound(
                code: "NotFoundFreeParkingSpotForDates",
                description: "No free parking spot found for dates.",
                metadata: new Dictionary<string, object> { { "Dates", dates } });
        }

        public static Error UserAlreadyHasReservationForDates(List<DateOnly> dates)
        {
            return Error.Conflict(
                code: "UserAlreadyHasReservationForDates",
                description: "User already has reservation for dates.",
                metadata: new Dictionary<string, object> { { "Dates", dates } });
        }
    }
}