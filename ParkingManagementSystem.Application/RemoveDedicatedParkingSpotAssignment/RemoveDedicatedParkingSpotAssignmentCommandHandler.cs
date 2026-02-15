using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.ParkingSpot.Enums;
using ParkingSpotErrors = ParkingManagementSystem.Domain.ParkingSpot.Errors.Errors.ParkingSpot;

namespace ParkingManagementSystem.Application.RemoveDedicatedParkingSpotAssignment;

public class
    RemoveDedicatedParkingSpotAssignmentCommandHandler : IRequestHandler<RemoveDedicatedParkingSpotAssignmentCommand,
    ErrorOr<Success>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IParkingSpotsRepository _parkingSpotsRepository;

    public RemoveDedicatedParkingSpotAssignmentCommandHandler(IUnitOfWork unitOfWork,
        IParkingSpotsRepository parkingSpotsRepository)
    {
        _unitOfWork = unitOfWork;
        _parkingSpotsRepository = parkingSpotsRepository;
    }

    public async Task<ErrorOr<Success>> Handle(RemoveDedicatedParkingSpotAssignmentCommand request,
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

            if (parkingSpot.ManagerId is null || parkingSpot.State != ParkingSpotState.Dedicated)
            {
                return ParkingSpotErrors.ParkingSpotIsNotDedicated();
            }

            parkingSpot.RemoveManagerAssignment();

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
}