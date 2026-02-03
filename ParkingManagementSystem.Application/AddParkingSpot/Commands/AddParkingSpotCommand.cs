using ErrorOr;
using MediatR;
using ParkingManagementSystem.Domain.AddParkingSpot;
using ParkingManagementSystem.Domain.AddParkingSpot.Enums;

namespace ParkingManagementSystem.Application.AddParkingSpot.Commands;

public record AddParkingSpotCommand(
    string Name,
    string Description,
    ParkingSpotState State) : IRequest<ErrorOr<ParkingSpot>>;