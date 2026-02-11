using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.Reservation;
using UserErrors = ParkingManagementSystem.Domain.User.Errors.Errors.User;
using ReservationErrors = ParkingManagementSystem.Domain.Reservation.Errors.Errors.Reservation;

namespace ParkingManagementSystem.Application.CancelReservation;

public class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand, ErrorOr<List<DateOnly>>>
{
    private readonly IReservationsRepository _reservationsRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelReservationCommandHandler(IReservationsRepository reservationsRepository, IUnitOfWork unitOfWork,
        IUserRepository userRepository)
    {
        _reservationsRepository = reservationsRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<ErrorOr<List<DateOnly>>> Handle(CancelReservationCommand request,
        CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var userExists = await _userRepository.ExistsAsync(request.UserId, cancellationToken);
            if (!userExists)
            {
                return UserErrors.UserNotFound();
            }

            var reservations =
                await _reservationsRepository.GetReservationsByIdsAsync(request.ReservationIds, cancellationToken);
            if (reservations.Count != request.ReservationIds.Count)
            {
                return ReservationErrors.ReservationNotFound();
            }

            var allReservationBelongsToUser = reservations.All(r => r.UserId == request.UserId);
            if (!allReservationBelongsToUser)
            {
                return Error.Unauthorized();
            }

            CancelReservations(reservations);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var cancelledDays = reservations.Select(r => r.ReservationDate).ToList();
            return cancelledDays;
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Error.Failure();
        }
    }

    private void CancelReservations(List<Reservation> reservations)
    {
        foreach (var reservation in reservations)
        {
            _reservationsRepository.Remove(reservation);
        }
    }
}