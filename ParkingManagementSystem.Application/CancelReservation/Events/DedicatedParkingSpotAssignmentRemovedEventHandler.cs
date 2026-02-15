using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.ParkingSpot.Events;
using ParkingManagementSystem.Domain.Reservation;

namespace ParkingManagementSystem.Application.CancelReservation.Events;

public class
    DedicatedParkingSpotAssignmentRemovedEventHandler : INotificationHandler<DedicatedParkingSpotAssignmentRemoved>
{
    private readonly IReservationsRepository _reservationsRepository;

    public DedicatedParkingSpotAssignmentRemovedEventHandler(IReservationsRepository reservationsRepository)
    {
        _reservationsRepository = reservationsRepository;
    }

    public async Task Handle(DedicatedParkingSpotAssignmentRemoved notification, CancellationToken cancellationToken)
    {
        var reservations =
            await _reservationsRepository.GetReservationsForParkingSpotFromTodayAsync(notification.ParkingSpotId,
                cancellationToken);

        CancelReservations(reservations);
    }

    private void CancelReservations(List<Reservation> reservations)
    {
        foreach (var reservation in reservations)
        {
            _reservationsRepository.Remove(reservation);
        }
    }
}