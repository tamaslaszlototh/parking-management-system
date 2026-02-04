namespace ParkingManagementSystem.Contracts.ParkingSpot.AddParkingSpot;

public record AddParkingSpotResult(Guid Id, string Name, string Description, ParkingSpotState State);