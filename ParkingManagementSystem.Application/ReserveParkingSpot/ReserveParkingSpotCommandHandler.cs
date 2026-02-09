using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.Reservation;
using ParkingSpotErrors = ParkingManagementSystem.Domain.ParkingSpot.Errors.Errors.ParkingSpot;
using UserErrors = ParkingManagementSystem.Domain.User.Errors.Errors.User;

namespace ParkingManagementSystem.Application.ReserveParkingSpot;

public class ReserveParkingSpotCommandHandler : IRequestHandler<ReserveParkingSpotCommand, ErrorOr<Success>>
{
    private readonly IReservationsRepository _reservationsRepository;
    private readonly IParkingSpotsRepository _parkingSpotsRepository;
    private readonly IUserRepository _userRepository;

    public ReserveParkingSpotCommandHandler(IReservationsRepository reservationsRepository,
        IParkingSpotsRepository parkingSpotsRepository, IUserRepository userRepository)
    {
        _reservationsRepository = reservationsRepository;
        _parkingSpotsRepository = parkingSpotsRepository;
        _userRepository = userRepository;
    }

    public async Task<ErrorOr<Success>> Handle(ReserveParkingSpotCommand request, CancellationToken cancellationToken)
    {
        var parkingSpotExists = await _parkingSpotsRepository.ExistsAsync(request.ParkingSpotId, cancellationToken);
        if (!parkingSpotExists)
        {
            return ParkingSpotErrors.ParkingSpotNotFound();
        }

        var userExists = await _userRepository.ExistsAsync(request.UserId, cancellationToken);
        if (!userExists)
        {
            return UserErrors.UserNotFound();
        }

        var parkingSpotReserved = await _reservationsRepository.HasReservationForAsync(
            request.ParkingSpotId, request.Date, cancellationToken);

        if (parkingSpotReserved)
        {
            return ParkingSpotErrors.ParkingSpotAlreadyReservedForDate(request.Date);
        }

        var reservation = Reservation.Create(request.UserId, request.ParkingSpotId, request.Date);

        await _reservationsRepository.AddAsync(reservation, cancellationToken);

        return Result.Success;
    }
}