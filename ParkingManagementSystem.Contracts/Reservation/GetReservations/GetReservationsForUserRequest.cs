namespace ParkingManagementSystem.Contracts.Reservation.GetReservations;

public record GetReservationsForUserRequest(Guid UserId, bool IncludeExpired = false);