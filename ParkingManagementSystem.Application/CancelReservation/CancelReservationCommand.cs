using ErrorOr;
using MediatR;

namespace ParkingManagementSystem.Application.CancelReservation;

public record CancelReservationCommand(
    List<Guid> ReservationIds,
    Guid UserId) : IRequest<ErrorOr<List<DateOnly>>>;