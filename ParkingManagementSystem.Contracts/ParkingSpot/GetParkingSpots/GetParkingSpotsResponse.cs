namespace ParkingManagementSystem.Contracts.ParkingSpot.GetParkingSpots;

public record GetParkingSpotsResponse(
    List<ParkingSpotsDto> ParkingSpots);

public record ParkingSpotsDto(
    Guid Id,
    string Name,
    string Description,
    ParkingSpotState State,
    DateOnly? LastReservedDate);