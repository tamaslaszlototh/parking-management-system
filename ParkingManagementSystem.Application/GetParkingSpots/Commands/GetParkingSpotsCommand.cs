using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.GetParkingSpots.Models;

namespace ParkingManagementSystem.Application.GetParkingSpots.Commands;

public record GetParkingSpotsCommand() : IRequest<ErrorOr<GetParkingSpotsResult>>;