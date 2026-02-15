using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.DeactivateParkingSpot.Models;

namespace ParkingManagementSystem.Application.DeactivateParkingSpot.Commands;

public record DeactivateParkingSpotCommand(Guid ParkingSpotId) : IRequest<ErrorOr<DeactivateParkingSpotCommandResult>>;