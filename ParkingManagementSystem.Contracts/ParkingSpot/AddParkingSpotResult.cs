namespace ParkingManagementSystem.Contracts.ParkingSpot;

public record AddParkingSpotResult(Guid Id, string Name, string Description, ParkingSpotState State);