using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.User.Events;

namespace ParkingManagementSystem.Application.AssignDedicatedParkingSpot.Events;

public class ParkingSpotAssignedEventHandlerForParkingSpot : INotificationHandler<ParkingSpotAssignedEvent>
{
    private readonly IParkingSpotsRepository _parkingSpotsRepository;

    public ParkingSpotAssignedEventHandlerForParkingSpot(IParkingSpotsRepository parkingSpotsRepository)
    {
        _parkingSpotsRepository = parkingSpotsRepository;
    }

    public async Task Handle(ParkingSpotAssignedEvent notification, CancellationToken cancellationToken)
    {
        var parkingSpot = await _parkingSpotsRepository.GetByIdAsync(notification.ParkingSpotId, cancellationToken);
        parkingSpot!.AssignManager(notification.ManagerId);
        _parkingSpotsRepository.Update(parkingSpot);
    }
}