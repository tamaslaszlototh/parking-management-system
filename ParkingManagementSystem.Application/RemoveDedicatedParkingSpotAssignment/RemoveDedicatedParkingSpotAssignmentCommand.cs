using ErrorOr;
using MediatR;

namespace ParkingManagementSystem.Application.RemoveDedicatedParkingSpotAssignment;

public record RemoveDedicatedParkingSpotAssignmentCommand(Guid ParkingSpotId) : IRequest<ErrorOr<Success>>;