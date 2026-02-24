using ParkingManagementSystem.Domain.Reservation;

namespace ParkingManagementSystem.Application.GetReservationsForDates.Models;

public record GetReservationsForDatesResult(List<Reservation> Reservations);