using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.GetParkingSpots.Models;

namespace ParkingManagementSystem.Application.GetParkingSpots.Commands;

public record GetParkingSpotsCommand(bool IncludeDeactivated = false) : IRequest<ErrorOr<GetParkingSpotsResult>>;