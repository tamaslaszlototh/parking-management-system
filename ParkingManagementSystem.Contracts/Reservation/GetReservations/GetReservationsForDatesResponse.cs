namespace ParkingManagementSystem.Contracts.Reservation.GetReservations;

public record GetReservationsForDatesResponse(List<ReservationDto> Reservations);

public record ReservationDto(Guid Id, Guid ParkingSpotId, DateOnly ReservationDate);