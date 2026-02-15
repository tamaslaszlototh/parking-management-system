namespace ParkingManagementSystem.Application.DeactivateParkingSpot.Models;

public record DeactivateParkingSpotCommandResult(List<Guid> ReservationIds, DateOnly? LastReservedDate);