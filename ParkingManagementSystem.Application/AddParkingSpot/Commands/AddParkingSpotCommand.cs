using ErrorOr;
using MediatR;
using ParkingManagementSystem.Domain.ParkingSpot;
using ParkingManagementSystem.Domain.ParkingSpot.Enums;

namespace ParkingManagementSystem.Application.AddParkingSpot.Commands;

public record AddParkingSpotCommand(
    string Name,
    string Description,
    ParkingSpotState State) : IRequest<ErrorOr<ParkingSpot>>;