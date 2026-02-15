using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Application.DeactivateParkingSpot.Models;
using ParkingManagementSystem.Domain.ParkingSpot.Enums;
using ParkingSpotErrors = ParkingManagementSystem.Domain.ParkingSpot.Errors.Errors.ParkingSpot;

namespace ParkingManagementSystem.Application.DeactivateParkingSpot.Commands;

public class DeactivateParkingSpotCommandHandler : IRequestHandler<DeactivateParkingSpotCommand,
    ErrorOr<DeactivateParkingSpotCommandResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IParkingSpotsRepository _parkingSpotsRepository;
    private readonly IReservationsRepository _reservationsRepository;

    public DeactivateParkingSpotCommandHandler(IUnitOfWork unitOfWork, IParkingSpotsRepository parkingSpotsRepository,
        IReservationsRepository reservationsRepository)
    {
        _unitOfWork = unitOfWork;
        _parkingSpotsRepository = parkingSpotsRepository;
        _reservationsRepository = reservationsRepository;
    }

    public async Task<ErrorOr<DeactivateParkingSpotCommandResult>> Handle(DeactivateParkingSpotCommand request,
        CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var parkingSpot = await _parkingSpotsRepository.GetByIdAsync(request.ParkingSpotId, cancellationToken);
            if (parkingSpot is null)
            {
                return ParkingSpotErrors.ParkingSpotNotFound();
            }

            if (parkingSpot.State == ParkingSpotState.Deactivated)
            {
                return ParkingSpotErrors.ParkingSpotIsAlreadyDeactivated();
            }

            parkingSpot.Deactivate();
            _parkingSpotsRepository.Update(parkingSpot);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var reservations =
                await _reservationsRepository.GetReservationsForParkingSpotFromTodayAsync(parkingSpot.Id,
                    cancellationToken);

            var reservationIds = reservations.Select(r => r.Id).ToList();
            var lastReservedDate = reservations.Count != 0
                ? reservations.Max(r => r.ReservationDate)
                : (DateOnly?)null;

            return new DeactivateParkingSpotCommandResult(reservationIds, lastReservedDate);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Error.Failure();
        }
    }
}