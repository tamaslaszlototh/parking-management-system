namespace ParkingManagementSystem.Contracts.Reservation.CancelReservation;

public record CancelReservationRequest(List<Guid> ReservationIds, Guid UserId);