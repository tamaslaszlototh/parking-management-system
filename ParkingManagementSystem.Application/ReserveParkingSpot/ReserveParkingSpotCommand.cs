using ErrorOr;
using MediatR;

namespace ParkingManagementSystem.Application.ReserveParkingSpot;

public record ReserveParkingSpotCommand(Guid UserId, List<DateOnly> Dates, bool PreferDedicatedParkingSpots = false)
    : IRequest<ErrorOr<Success>>;