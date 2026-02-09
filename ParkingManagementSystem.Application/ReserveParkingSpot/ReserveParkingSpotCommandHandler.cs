using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.ParkingSpot.Errors;
using ParkingManagementSystem.Domain.Reservation;

namespace ParkingManagementSystem.Application.ReserveParkingSpot;

public class ReserveParkingSpotCommandHandler : IRequestHandler<ReserveParkingSpotCommand, ErrorOr<Success>>
{
    private readonly IReservationsRepository _reservationsRepository;

    public ReserveParkingSpotCommandHandler(IReservationsRepository reservationsRepository)
    {
        _reservationsRepository = reservationsRepository;
    }

    public async Task<ErrorOr<Success>> Handle(ReserveParkingSpotCommand request, CancellationToken cancellationToken)
    {
        var parkingSpotReserved = await _reservationsRepository.HasReservationForAsync(
            request.ParkingSpotId, request.Date, cancellationToken);

        if (parkingSpotReserved)
        {
            return Errors.ParkingSpot.ParkingSpotAlreadyReservedForDate(request.Date);
        }

        var reservation = Reservation.Create(request.UserId, request.ParkingSpotId, request.Date);

        await _reservationsRepository.AddAsync(reservation, cancellationToken);

        return Result.Success;
    }
}