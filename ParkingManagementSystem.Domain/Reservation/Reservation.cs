using ParkingManagementSystem.Domain.Common;

namespace ParkingManagementSystem.Domain.Reservation;

public class Reservation : AggregateRoot
{
    public Guid UserId { get; }
    public Guid ParkingSpotId { get; }
    public DateOnly ReservationDate { get; }

    private Reservation(Guid id, Guid userId, Guid parkingSpotId, DateOnly reservationDate) : base(id)
    {
        UserId = userId;
        ParkingSpotId = parkingSpotId;
        ReservationDate = reservationDate;
    }

    public static Reservation Create(Guid userId, Guid parkingSpotId, DateOnly reservationDate, Guid? id = null)
    {
        var reservationId = id ?? Guid.NewGuid();
        return new Reservation(reservationId, userId, parkingSpotId, reservationDate);
    }
}