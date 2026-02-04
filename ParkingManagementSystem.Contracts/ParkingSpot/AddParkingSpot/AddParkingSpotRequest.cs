namespace ParkingManagementSystem.Contracts.ParkingSpot.AddParkingSpot;

public record AddParkingSpotRequest(string Name, string? Description, ParkingSpotState? State);