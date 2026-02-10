namespace ParkingManagementSystem.Contracts.Reservation.ReserveParkingSpot;

public record ReserveParkingSpotRequest(Guid UserId, List<DateOnly> Dates);