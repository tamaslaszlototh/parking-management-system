namespace ParkingManagementSystem.Application.GetParkingSpots.Models;

public record GetParkingSpotsResult(
    List<ParkingSpotDto> ParkingSpots);

public record ParkingSpotDto(Guid Id, string Name, string? Description);