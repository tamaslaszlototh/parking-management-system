using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.ParkingSpot;
using ParkingManagementSystem.Domain.Reservation;
using ParkingManagementSystem.Domain.Reservation.Errors;
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
        var userExists = await _userRepository.ExistsAsync(request.UserId, cancellationToken);
        if (!userExists)
        {
            return UserErrors.UserNotFound();
        }

        var userHasReservationForDate =
            await _reservationsRepository.HasReservationForAsync(request.UserId, request.Date, cancellationToken);

        if (userHasReservationForDate)
        {
            return Errors.Reservation.UserAlreadyHasReservationForDate(request.Date);
        }

        var availableParkingSpots = await _parkingSpotsRepository.GetNotDeactivatedParkingSpotsAsync(cancellationToken);

        var availableParkingSpotCount = availableParkingSpots.Count;
        var counter = 0;
        ParkingSpot? freeParkingSpot = null;

        while (freeParkingSpot is null && counter < availableParkingSpotCount)
        {
            var parkingSpot = availableParkingSpots[counter];
            var isParkingSpotFree =
                await _parkingSpotsRepository.FreeForReservationFor(request.Date, request.UserId, parkingSpot.Id,
                    cancellationToken);
            if (isParkingSpotFree)
            {
                freeParkingSpot = parkingSpot;
            }

            counter++;
        }

        if (freeParkingSpot is null)
        {
            return Errors.Reservation.NotFoundFreeParkingSpot(request.Date);
        }


        var reservation = Reservation.Create(request.UserId, freeParkingSpot.Id, request.Date);

        await _reservationsRepository.AddAsync(reservation, cancellationToken);

        return Result.Success;
    }
}