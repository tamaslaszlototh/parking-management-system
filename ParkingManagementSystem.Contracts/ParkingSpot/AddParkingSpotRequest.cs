namespace ParkingManagementSystem.Contracts.ParkingSpot;

public record AddParkingSpotRequest(string Name, string? Description, ParkingSpotState? State);