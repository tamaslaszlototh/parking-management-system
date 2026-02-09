using ErrorOr;
using MediatR;

namespace ParkingManagementSystem.Application.ReserveParkingSpot;

public record ReserveParkingSpotCommand(Guid UserId, DateOnly Date) : IRequest<ErrorOr<Success>>;