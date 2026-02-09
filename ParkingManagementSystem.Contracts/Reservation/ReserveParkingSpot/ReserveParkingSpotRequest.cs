namespace ParkingManagementSystem.Contracts.Reservation.ReserveParkingSpot;

public record ReserveParkingSpotRequest(Guid UserId, DateOnly Date);