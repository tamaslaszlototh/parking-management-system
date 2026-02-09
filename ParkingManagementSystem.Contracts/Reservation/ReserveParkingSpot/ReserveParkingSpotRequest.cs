namespace ParkingManagementSystem.Contracts.Reservation.ReserveParkingSpot;

public record ReserveParkingSpotRequest(Guid UserId, Guid ParkingSpotId, DateOnly Date);