using ErrorOr;
using MediatR;

namespace ParkingManagementSystem.Application.AssignDedicatedParkingSpot;

public record AssignDedicatedParkingSpotCommand(
    Guid ManagerId,
    Guid ParkingSpotId) : IRequest<ErrorOr<Success>>;