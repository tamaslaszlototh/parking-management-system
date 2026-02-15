namespace ParkingManagementSystem.Contracts.ParkingSpot.DeactivateParkingSpot;

public record DeactivateParkingSpotResponse(List<Guid> ReservationIds, DateOnly LastReservedDate);