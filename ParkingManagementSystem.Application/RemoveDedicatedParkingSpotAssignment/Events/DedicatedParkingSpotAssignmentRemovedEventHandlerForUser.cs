using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.ParkingSpot.Events;

namespace ParkingManagementSystem.Application.RemoveDedicatedParkingSpotAssignment.Events;

public class
    DedicatedParkingSpotAssignmentRemovedEventHandlerForUser : INotificationHandler<
    DedicatedParkingSpotAssignmentRemoved>
{
    private readonly IUserRepository _userRepository;

    public DedicatedParkingSpotAssignmentRemovedEventHandlerForUser(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(DedicatedParkingSpotAssignmentRemoved notification, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByAssignedParkingSpotIdAsync(notification.ParkingSpotId, cancellationToken);
        if (user is null) return;
        
        user.RemoveParkingSpotAssignment();
        _userRepository.Update(user);
    }
}