namespace ParkingManagementSystem.Contracts.Reservation.GetReservations;

public record GetReservationsForDatesRequest(List<DateOnly> Dates);