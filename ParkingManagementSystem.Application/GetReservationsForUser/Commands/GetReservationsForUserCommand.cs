using MediatR;
using ParkingManagementSystem.Application.GetReservationsForUser.Models;
using ErrorOr;

namespace ParkingManagementSystem.Application.GetReservationsForUser.Commands;

public record GetReservationsForUserCommand(Guid UserId, bool IncludeExpired = false)
    : IRequest<ErrorOr<GetReservationsForUserResult>>;