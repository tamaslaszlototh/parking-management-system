namespace ParkingManagementSystem.Contracts.Reservation.CancelReservation;

public record CancelReservationResponse(List<DateOnly> CancelledDates);