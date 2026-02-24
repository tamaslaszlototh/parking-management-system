using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.ParkingSpot;
using ParkingManagementSystem.Domain.ParkingSpot.Enums;
using ParkingManagementSystem.Domain.Reservation;
using ParkingManagementSystem.Domain.Reservation.Errors;
using UserErrors = ParkingManagementSystem.Domain.User.Errors.Errors.User;

namespace ParkingManagementSystem.Application.ReserveParkingSpot;

public class ReserveParkingSpotCommandHandler : IRequestHandler<ReserveParkingSpotCommand, ErrorOr<Success>>
{
    private readonly IReservationsRepository _reservationsRepository;
    private readonly IParkingSpotsRepository _parkingSpotsRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReserveParkingSpotCommandHandler(IReservationsRepository reservationsRepository,
        IParkingSpotsRepository parkingSpotsRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _reservationsRepository = reservationsRepository;
        _parkingSpotsRepository = parkingSpotsRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(ReserveParkingSpotCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var userExists = await _userRepository.ExistsAsync(request.UserId, cancellationToken);
            if (!userExists)
            {
                return UserErrors.UserNotFound();
            }

            var reservationCheckResult =
                await CheckUserReservationsForDates(request.UserId, request.Dates, cancellationToken);

            if (reservationCheckResult.hasReservation)
            {
                return Errors.Reservation.UserAlreadyHasReservationForDates(reservationCheckResult.reservedDates);
            }

            var freeParkingSpotResult = await FindFreeParkingSpots(request.Dates, request.UserId,
                request.PreferDedicatedParkingSpots, cancellationToken);

            if (freeParkingSpotResult.Count != request.Dates.Count)
            {
                return Errors.Reservation.NotFoundFreeParkingSpotForDates(request.Dates);
            }

            await ReserveParkingSpots(request.UserId, freeParkingSpotResult, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Error.Failure();
        }
    }

    private async Task ReserveParkingSpots(Guid userId, List<(ParkingSpot, DateOnly)> freeParkingSpots,
        CancellationToken cancellationToken)
    {
        foreach (var (parkingSpot, date) in freeParkingSpots)
        {
            var reservation = Reservation.Create(userId, parkingSpot.Id, date);
            await _reservationsRepository.AddAsync(reservation, cancellationToken);
        }
    }

    private async Task<List<(ParkingSpot, DateOnly)>> FindFreeParkingSpots(List<DateOnly> dates, Guid userId,
        bool preferDedicatedParkingSpots, CancellationToken cancellationToken)
    {
        List<(ParkingSpot, DateOnly)> freeParkingSpots = [];
        foreach (var date in dates)
        {
            var freeParkingSpot =
                await FindFreeParkingSpot(date, userId, preferDedicatedParkingSpots, cancellationToken);
            if (freeParkingSpot != null)
            {
                freeParkingSpots.Add((freeParkingSpot, date));
            }
        }

        return freeParkingSpots;
    }

    private async Task<ParkingSpot?> FindFreeParkingSpot(DateOnly date, Guid userId, bool preferDedicatedParkingSpots,
        CancellationToken cancellationToken)
    {
        var availableParkingSpots = await _parkingSpotsRepository.GetNotDeactivatedParkingSpotsAsync(cancellationToken);
        var reservedParkingSpotIds =
            await _reservationsRepository.GetReservedParkingSpotsForDateAsync(date, cancellationToken);

        if (preferDedicatedParkingSpots)
        {
            var freeDedicatedParkingSpot = availableParkingSpots.FirstOrDefault(p =>
                p.ManagerId == userId
                && p.State != ParkingSpotState.Deactivated
                && !reservedParkingSpotIds.Contains(p.Id));

            return freeDedicatedParkingSpot ??
                   GetFirstFreeParkingSpot(availableParkingSpots, reservedParkingSpotIds, userId);
        }

        return GetFirstFreeParkingSpot(availableParkingSpots, reservedParkingSpotIds, userId);
    }

    private async Task<(bool hasReservation, List<DateOnly> reservedDates)> CheckUserReservationsForDates(Guid userId,
        List<DateOnly> dates,
        CancellationToken cancellationToken)
    {
        List<DateOnly> reservedDates = [];
        foreach (var date in dates)
        {
            var userHasReservationForDate =
                await _reservationsRepository.HasReservationForAsync(userId, date, cancellationToken);

            if (userHasReservationForDate)
            {
                reservedDates.Add(date);
            }
        }

        return (reservedDates.Count > 0, reservedDates);
    }

    private ParkingSpot? GetFirstFreeParkingSpot(List<ParkingSpot> availableParkingSpots,
        List<Guid> reservedParkingSpotIds, Guid userId)
    {
        var rs = availableParkingSpots.FirstOrDefault(p =>
            !reservedParkingSpotIds.Contains(p.Id)
            && p.State is ParkingSpotState.Active or ParkingSpotState.Dedicated);
        return rs;
    }
}