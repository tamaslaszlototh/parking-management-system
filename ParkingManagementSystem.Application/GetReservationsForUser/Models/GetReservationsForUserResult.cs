using ParkingManagementSystem.Domain.Reservation;

namespace ParkingManagementSystem.Application.GetReservationsForUser.Models;

public record GetReservationsForUserResult(List<Reservation> Reservations);