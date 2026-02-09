using ErrorOr;
using MediatR;

namespace ParkingManagementSystem.Application.ReserveParkingSpot;

public record ReserveParkingSpotCommand(Guid UserId, Guid ParkingSpotId, DateOnly Date) : IRequest<ErrorOr<Success>>;