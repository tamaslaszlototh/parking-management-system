using ErrorOr;
using MediatR;

namespace ParkingManagementSystem.Application.ReserveParkingSpot;

public record ReserveParkingSpotCommand(Guid UserId, List<DateOnly> Dates) : IRequest<ErrorOr<Success>>;