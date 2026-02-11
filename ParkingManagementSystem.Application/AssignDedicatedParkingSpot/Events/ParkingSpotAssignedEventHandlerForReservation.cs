using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.Reservation;
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
            CancelReservations(reservations);
        }

        var newReservations = CreateReservations(notification.ManagerId, notification.ParkingSpotId);

        foreach (var reservation in newReservations)
        {
            await _reservationsRepository.AddAsync(reservation, cancellationToken);
        }
    }

    private void CancelReservations(List<Reservation> reservations)
    {
        foreach (var reservation in reservations)
        {
            _reservationsRepository.Remove(reservation);
        }
    }

    private List<Reservation> CreateReservations(Guid managerId, Guid parkingSpotId)
    {
        List<Reservation> reservations = [];
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var endOfYear = new DateOnly(today.Year, 12, 31);

        for (var date = today; date <= endOfYear; date = date.AddDays(1))
        {
            var reservation = Reservation.Create(managerId, parkingSpotId, date);
            reservations.Add(reservation);
        }

        return reservations;
    }
}