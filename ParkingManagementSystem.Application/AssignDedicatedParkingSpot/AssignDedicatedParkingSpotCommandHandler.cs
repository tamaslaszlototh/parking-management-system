using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.ParkingSpot.Enums;
using ParkingManagementSystem.Domain.User.Enums;
using UserErrors = ParkingManagementSystem.Domain.User.Errors.Errors.User;
using ParkingSpotErrors = ParkingManagementSystem.Domain.ParkingSpot.Errors.Errors.ParkingSpot;

namespace ParkingManagementSystem.Application.AssignDedicatedParkingSpot;

public class
    AssignDedicatedParkingSpotCommandHandler : IRequestHandler<AssignDedicatedParkingSpotCommand, ErrorOr<Success>>
{
    private readonly IUserRepository _userRepository;
    private readonly IParkingSpotsRepository _parkingSpotsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignDedicatedParkingSpotCommandHandler(IUserRepository userRepository,
        IParkingSpotsRepository parkingSpotsRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _parkingSpotsRepository = parkingSpotsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(AssignDedicatedParkingSpotCommand request,
        CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var parkingSpot = await _parkingSpotsRepository.GetByIdAsync(request.ParkingSpotId, cancellationToken);
            if (parkingSpot is not { State: ParkingSpotState.Dedicated })
            {
                return ParkingSpotErrors.ParkingSpotIsNotDedicated();
            }

            if (parkingSpot is not { ManagerId: null })
            {
                return ParkingSpotErrors.ParkingSpotIsAlreadyAssigned();
            }

            var manager = await _userRepository.GetByIdAsync(request.ManagerId, cancellationToken);
            if (manager is not { Role: UserRole.BusinessManager })
            {
                return UserErrors.ManagerNotFound();
            }

            if (manager.AssignedParkingSpotId is not null)
            {
                return UserErrors.ManagerIsAlreadyAssignedToParkingSpot();
            }

            manager.AssignParkingSpot(parkingSpot.Id);
            _userRepository.Update(manager);
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