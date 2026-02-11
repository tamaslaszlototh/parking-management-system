using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.User.Events;

namespace ParkingManagementSystem.Application.AssignDedicatedParkingSpot.Events;

public class ParkingSpotAssignedEventHandlerForReservation : INotificationHandler<ParkingSpotAssignedEvent>
{
    private readonly IReservationsRepository _reservationsRepository;

    public ParkingSpotAssignedEventHandlerForReservation(IReservationsRepository reservationsRepository)
    {
        _reservationsRepository = reservationsRepository;
    }

    public async Task Handle(ParkingSpotAssignedEvent notification, CancellationToken cancellationToken)
    {
        var reservations =
            await _reservationsRepository.GetActiveReservationsAsync(notification.ManagerId, cancellationToken);

        if (reservations.Count != 0)
        {
            
        }

        //remove manager's reservations
        //reserve dedicated parking spot for the rest of the year
    }
}